using Sidekick.Common.Game.Items.Modifiers;

namespace Sidekick.Domain.Game.Trade.Models
{
    public class ModifierFilter
    {
        public Modifier Modifier { get; set; }

        public bool Enabled { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }
    }
}
