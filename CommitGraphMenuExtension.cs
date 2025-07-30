// Required namespaces from Mendix Studio Pro Extensions API
using Mendix.StudioPro.ExtensionsAPI.UI.Menu;         // For creating custom menu entries in the Mendix Studio Pro UI
using Mendix.StudioPro.ExtensionsAPI.UI.Services;     // For services like docking windows and message boxes
using System.ComponentModel.Composition;              // For MEF (Managed Extensibility Framework) attributes

namespace CommitGraph
{
    // Export this class as a MenuExtension so Mendix recognizes it as a new menu extension
    [Export(typeof(MenuExtension))]
    public class CommitGraphMenuExtension : MenuExtension
    {
        // Service to manage docking windows (like tool windows or side panels)
        private readonly IDockingWindowService _dockingWindowService;

        // Service to show informational or error messages in the Mendix IDE
        private readonly IMessageBoxService _messageBoxService;

        // Constructor that gets required services injected by Mendix using [ImportingConstructor]
        [ImportingConstructor]
        public CommitGraphMenuExtension(IDockingWindowService dockingWindowService, IMessageBoxService messageBoxService)
        {
            _dockingWindowService = dockingWindowService;
            _messageBoxService = messageBoxService;
        }

        // This method defines the menu items added to Mendix Studio Pro
        public override IEnumerable<MenuViewModel> GetMenus()
        {
            // Add a single menu item called "Git Graph (In-App)"
            yield return new MenuViewModel("Git Graph (In-App)", () =>
            {
                try
                {
                    // Show a message when the menu item is clicked
                    _messageBoxService.ShowInformation("Opening Git Graph pane...");

                    // Open the custom Git Graph pane using its registered ID
                    _dockingWindowService.OpenPane(GitGraphPaneExtension.ID);
                }
                catch (Exception ex)
                {
                    // If anything goes wrong show an error message
                    _messageBoxService.ShowError($"Failed to open Git Graph pane: {ex.Message}");
                }
            });
        }
    }
}
