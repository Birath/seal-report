using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.DesignTokens;

namespace TitleReport.Shared
{
	public partial class MainLayout
	{
        ErrorBoundary? _errorBoundary;
        FluentDesignSystemProvider _fdsp = new();
        LocalizationDirection? _dir = LocalizationDirection.ltr;

        private void SwitchTheme()
        {
            _activeTheme = _activeTheme.Type == ThemeType.Dark ? Theme.Light : Theme.Dark;
            IsLightTheme = _activeTheme.Type == ThemeType.Light;
            LocalStorage.SetItemAsync(nameof(_activeTheme), _activeTheme.Type);
        }

        protected override void OnParametersSet()
        {
            _errorBoundary?.Recover();
        }
    }

    public class Theme
    {
        public ThemeType Type { get; set; }
        public float BaseLayerLuminance { get; set; }
        public Theme() { }

        private Theme(ThemeType type, float baseLayerLuminance)
        {
            Type = type;
            BaseLayerLuminance = baseLayerLuminance;
        }

        public static readonly Theme Dark = new(ThemeType.Dark, 0.2f);
        public static readonly Theme Light = new(ThemeType.Light, 1.0f);
    }

    public enum ThemeType
    {
        Light,
        Dark,
    }
}
