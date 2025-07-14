using System;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class HomeView : MainNavigablePageBase
{
    private ImplicitAnimationCollection? _implicitAnimations;
    public HomeView() : base("主页", LucideIconKind.House, NavigationPageCategory.Top, 0)
    {
        InitializeComponent();
        DataContext = App.GetService<HomeViewModel>();
    }

    private void EnsureImplicitAnimations()
    {
        if (_implicitAnimations != null) return;
        var compositor = ElementComposition.GetElementVisual(this)!.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.Target = "RotationAngle";
        rotationAnimation.InsertKeyFrame(.5f, 0.160f);
        rotationAnimation.InsertKeyFrame(1f, 0f);
        rotationAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(offsetAnimation);
        animationGroup.Add(rotationAnimation);

        _implicitAnimations = compositor.CreateImplicitAnimationCollection();
        _implicitAnimations["Offset"] = animationGroup;
    }

    public static void SetEnableAnimations(Border border, bool value)
    {
        var page = border.FindAncestorOfType<HomeView>();
        if (page == null)
        {
            border.AttachedToVisualTree += delegate { SetEnableAnimations(border, true); };
            return;
        }

        if (ElementComposition.GetElementVisual(page) == null)
            return;

        page.EnsureImplicitAnimations();
        if (border.GetVisualParent() is { } visualParent 
            && ElementComposition.GetElementVisual(visualParent) is { } compositionVisual)
        {
            compositionVisual.ImplicitAnimations = page._implicitAnimations;
        }
    }
}