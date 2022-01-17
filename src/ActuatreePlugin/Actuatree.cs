using Mono.Cecil.Cil;
using MonoMod.Cil;
using OTAPI.Tile;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;

namespace ActuatreePlugin
{
    [ApiVersion(2, 1)]
    public partial class Actuatree : TerrariaPlugin
    {
        /// <inheritdoc />
        public override string Name => typeof(Actuatree).Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

        /// <inheritdoc />
        public override string Description => typeof(Actuatree).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        /// <inheritdoc />
        public override Version Version => typeof(Actuatree).Assembly.GetName().Version;

        /// <inheritdoc />
        public override string Author => typeof(Actuatree).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

        public Actuatree(Main game) : base(game)
        {
        }

        private void WorldGen_CheckTree(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // go to 2nd num4 != 2
            c.GotoNext(ins => ins.OpCode == OpCodes.Ldc_I4_2 && ins.Next.OpCode == OpCodes.Beq_S);
            c.GotoNext(MoveType.After, ins => ins.OpCode == OpCodes.Ldc_I4_2 && ins.Next.OpCode == OpCodes.Beq_S);

            ILLabel lAfterIf;
            ILLabel lInIf = c.DefineLabel();

            // Replace beq.s with bne.un.s
            c.Emit(OpCodes.Bne_Un_S, lInIf);
            lAfterIf = (ILLabel)c.Next.Operand;
            c.Remove();

            // Insert Patch
            // || Main.tile[i, j + 1].inActive()
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.tile)));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Add);
            c.Emit(OpCodes.Callvirt, typeof(ITileCollection).GetProperties().Single(p => p.GetIndexParameters().Length == 2).GetGetMethod());
            c.Emit(OpCodes.Callvirt, typeof(ITile).GetMethod(nameof(ITile.inActive), Array.Empty<Type>()));
            c.Emit(OpCodes.Brfalse_S, lAfterIf);

            c.MarkLabel(lInIf);

            // Update client
            // NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0)
            c.GotoNext(MoveType.After, ins => ins.MatchCall(typeof(WorldGen), nameof(WorldGen.KillTile)));
            c.Emit(OpCodes.Ldc_I4_S, (sbyte)17);
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Conv_R4);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Conv_R4);
            c.Emit(OpCodes.Ldc_R4, 0f);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Call, typeof(NetMessage).GetMethod(nameof(NetMessage.SendData)));
        }

        public override void Initialize()
        {
            Initialize_Credits();

            IL.Terraria.WorldGen.CheckTree += WorldGen_CheckTree;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
