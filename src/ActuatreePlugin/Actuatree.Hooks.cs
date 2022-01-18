using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ActuatreePlugin
{
    partial class Actuatree
    {
        private readonly List<IDetour> _detours = new();

        private void Initialize_Hooks()
        {
            var checkTreeMethod = ((Action<int, int>)WorldGen.CheckTree).Method;
            _detours.Add(new ILHook(checkTreeMethod, WorldGen_CheckTree));
        }

        private void Dispose_Hooks()
        {
            for (int i = 0; i < _detours.Count; i++)
            {
                var detour = _detours[i];
                detour.Dispose();
            }
        }

        private void WorldGen_CheckTree(ILContext il)
        {
            ILCursor c = new(il);

            // Works as of 1.4.3.2

            // What is CheckTree(int x, int y)?
            // This method checks if the block is a valid tree block
            // in terms of being connected to valid surfaces

            // What is num4 and why do we care about it?
            // num4 is the type of the block below the provided x, y(from the parameters)

            Func<Instruction, bool> neq2 = (ins) => ins.OpCode == OpCodes.Ldc_I4_2 && ins.Next.OpCode == OpCodes.Beq_S;
            c.GotoNext(neq2);
            // This is where the root of the tree is checked
            c.GotoNext(MoveType.After, neq2);

            // Steal after-if-statement label from next instruction
            ILLabel lAfterIf;
            ILLabel lInIf = c.DefineLabel();

            c.MoveAfterLabels();
            /*
             * The if statement is something like this:
             * if (num4 != 2) KillTile(i, j);
             */
            /*
             * Replace beq.s is jump over if-statement when num4 == 2
             * basically how if (num4 != 2) { ... } is written
             * What we want instead is if (num4 != 2 || tileBelowIsInActive) { ... }
             * So the first step is replace beq.s with bne.un.s so that
             * If it is num4 != 2, then it jumps inside, but if it's num4 == 2,
             * instead of skipping, it goes to the next condition like ||
             */
            // We will mark the label later
            c.Emit(OpCodes.Bne_Un_S, lInIf);
            // Steal the after-if label before removing the instruction
            lAfterIf = (ILLabel)c.Next.Operand;
            // Remove beq.s
            c.Remove();

            /* Insert Patch */
            /* || Main.tile[i, j + 1].inActive() */
            // Main.tile
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.tile)));
            // i
            c.Emit(OpCodes.Ldarg_0);
            // j
            c.Emit(OpCodes.Ldarg_1);
            // 1
            c.Emit(OpCodes.Ldc_I4_1);
            // ^ j + 1
            c.Emit(OpCodes.Add);
            // ^ Main.tile[i, j + 1]
            c.Emit(OpCodes.Callvirt, typeof(ITileCollection).GetProperties().Single(p => p.GetIndexParameters().Length == 2).GetGetMethod());
            // ^ Main.tile[i, j + 1].inActive()
            c.Emit(OpCodes.Callvirt, typeof(ITile).GetMethod(nameof(ITile.inActive), Array.Empty<Type>()));
            // ^ || Main.tile[i, j + 1].inActive()
            c.Emit(OpCodes.Brfalse_S, lAfterIf);
            /*
             * If the tile below is not actuated, then
             * jump over if statement, otherwise enter if statement
             */

            // We're inside the if statement, so let's mark the inside-if label
            c.MarkLabel(lInIf);



            /* Insert network broadcast code to update clients */
            /* NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0) */
            // Move after the tile is killed
            c.GotoNext(MoveType.After, ins => ins.MatchCall(typeof(WorldGen), nameof(WorldGen.KillTile)));
            // 17 (ID of modify tile packet)
            c.Emit(OpCodes.Ldc_I4_S, (sbyte)17);
            // -1 (send to everyone)
            c.Emit(OpCodes.Ldc_I4_M1);
            // -1 (ignore no one)
            c.Emit(OpCodes.Ldc_I4_M1);
            // null (no text required for this packet)
            c.Emit(OpCodes.Ldnull);
            // 0 (value for kill tile)
            c.Emit(OpCodes.Ldc_I4_0);
            // i
            c.Emit(OpCodes.Ldarg_0);
            // ^ (float)i (we do this to meet the method signature)
            c.Emit(OpCodes.Conv_R4);
            // j
            c.Emit(OpCodes.Ldarg_1);
            // ^ (float)j (same reason as above)
            c.Emit(OpCodes.Conv_R4);
            // 0 or false, we don't want to discard items from cascade tree break
            // We're pushing a float because we want to meet the method signature
            c.Emit(OpCodes.Ldc_R4, 0f);
            // 0 (useless parameter, using default value)
            c.Emit(OpCodes.Ldc_I4_0);
            // 0 (useless parameter, using default value)
            c.Emit(OpCodes.Ldc_I4_0);
            // 0 (useless parameter, using default value)
            c.Emit(OpCodes.Ldc_I4_0);
            // ^ NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0)
            c.Emit(OpCodes.Call, typeof(NetMessage).GetMethod(nameof(NetMessage.SendData)));
        }
    }
}
