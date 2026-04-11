using System.Linq;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public static class ResourceReadoutStateHelper
    {
        public static ResourceReadoutState state = new ResourceReadoutState();

        private static bool stateRestored = false;

        public const int ResourceReadoutOpenMask = 32;

        public static void SaveCurrentState()
        {
            state.categoryOpenBits.Clear();
            var rootCategories = DefDatabase<ThingCategoryDef>.AllDefs.Where(cat => cat.resourceReadoutRoot);
            foreach (var catDef in rootCategories)
            {
                SaveNodeState(catDef.treeNode);
            }
        }

        private static void SaveNodeState(TreeNode_ThingCategory node)
        {
            state.categoryOpenBits[node.catDef.defName] = node.openBits;
            foreach (var childNode in node.ChildCategoryNodes)
            {
                if (!childNode.catDef.resourceReadoutRoot)
                {
                    SaveNodeState(childNode);
                }
            }
        }

        public static void RestoreState()
        {
            if (stateRestored) return;
            
            var rootCategories = DefDatabase<ThingCategoryDef>.AllDefs.Where(cat => cat.resourceReadoutRoot);
            foreach (var catDef in rootCategories)
            {
                RestoreNodeState(catDef.treeNode);
            }
            stateRestored = true;
        }

        private static void RestoreNodeState(TreeNode_ThingCategory node)
        {
            if (state.categoryOpenBits.TryGetValue(node.catDef.defName, out var openBits))
            {
                node.openBits = openBits;
            }
            foreach (var childNode in node.ChildCategoryNodes)
            {
                if (!childNode.catDef.resourceReadoutRoot)
                {
                    RestoreNodeState(childNode);
                }
            }
        }

        public static void ResetRestoreFlag()
        {
            stateRestored = false;
        }
    }
}
