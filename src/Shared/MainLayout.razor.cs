using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.DesignTokens;

namespace TitleReport.Shared
{
	public partial class MainLayout
	{
        ErrorBoundary? errorBoundary;
        FluentDesignSystemProvider fdsp = new();
        LocalizationDirection? dir;
        float? baseLayerLuminance;


        public void SwitchTheme()
        {
            baseLayerLuminance = baseLayerLuminance == 0.2f ? 1 : 0.2f;
        }

        protected override void OnParametersSet()
        {
            errorBoundary?.Recover();
        }
    }
}
