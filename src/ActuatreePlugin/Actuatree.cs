using System;
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

        public override void Initialize()
        {
            Initialize_Credits();
            Initialize_Hooks();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dispose_Hooks();
            }
            base.Dispose(disposing);
        }
    }
}
