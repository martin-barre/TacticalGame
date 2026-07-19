using UnityEditor;

namespace Editor
{
    [CustomPropertyDrawer(typeof(SpellZone), true)]
    public class SpellZoneDrawer : ManagedReferenceDrawer<SpellZone>
    {
        protected override string SelectPrompt => "Select Spell Zone";
        protected override string EmptyPrompt => "No Spell Zone Types Available";
        protected override string UndoLabel => "Change Spell Zone Type";
    }
}
