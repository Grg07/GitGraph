using Mendix.StudioPro.ExtensionsAPI.UI.Menu;
using Mendix.StudioPro.ExtensionsAPI.UI.Services;
using System.ComponentModel.Composition;

namespace CommitGraph
{
    [Export(typeof(MenuExtension))]
    public class CommitGraphMenuExtension : MenuExtension
    {
        private readonly IDockingWindowService _dockingWindowService;
        private readonly IMessageBoxService _messageBoxService;

        [ImportingConstructor]
        public CommitGraphMenuExtension(IDockingWindowService dockingWindowService, IMessageBoxService messageBoxService)
        {
            _dockingWindowService = dockingWindowService;
            _messageBoxService = messageBoxService;
        }

        public override IEnumerable<MenuViewModel> GetMenus()
        {
            yield return new MenuViewModel("Git Graph (In-App)", () => {
                try {
                    _messageBoxService.ShowInformation("Opening Git Graph pane...");
                    _dockingWindowService.OpenPane(GitGraphPaneExtension.ID);
                }
                catch (Exception ex) {
                    _messageBoxService.ShowError($"Failed to open Git Graph pane: {ex.Message}");
                }
            });
        }
    }
}