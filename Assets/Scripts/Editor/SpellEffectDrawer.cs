using UnityEditor;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ServerEffectBase), true)]
    public class SpellEffectDrawer : ManagedReferenceDrawer<ServerEffectBase>
    {
        protected override string SelectPrompt => "Select Effect Type";
        protected override string EmptyPrompt => "No Effect Types Available";
        protected override string UndoLabel => "Change Effect Type";
    }
}
