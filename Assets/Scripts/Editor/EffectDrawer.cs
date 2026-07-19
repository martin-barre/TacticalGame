using UnityEditor;

namespace Editor
{
    [CustomPropertyDrawer(typeof(BuffEffect), true)]
    public class EffectDrawer : ManagedReferenceDrawer<BuffEffect>
    {
        protected override string SelectPrompt => "Select Effect Type";
        protected override string EmptyPrompt => "No Effect Types Available";
        protected override string UndoLabel => "Change Effect Type";
    }
}
