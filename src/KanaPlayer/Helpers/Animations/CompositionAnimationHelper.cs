﻿using System;
using Avalonia.Rendering.Composition;

namespace KanaPlayer.Helpers.Animations;

public static class CompositionAnimationHelper
{
    public static void MakeScrollable(CompositionVisual? compositionVisual, double milliseconds = 250)
    {
        if (compositionVisual == null)
            return;

        var compositor = compositionVisual.Compositor;
        var animationGroup = compositor.CreateAnimationGroup();
        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";

        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        animationGroup.Add(offsetAnimation);
        implicitAnimationCollection["Offset"] = animationGroup;
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }

    public static void MakeOpacityAnimated(CompositionVisual? compositionVisual, double milliseconds = 700)
    {
        if (compositionVisual == null)
            return;

        var compositor = compositionVisual.Compositor;
        var animationGroup = compositor.CreateAnimationGroup();
        
        var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = "Opacity";
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        opacityAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        
        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        
        animationGroup.Add(offsetAnimation);
        animationGroup.Add(opacityAnimation);
        
        var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Opacity"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;
        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }

    public static void MakeSizeAnimated(CompositionVisual? compositionVisual, double milliseconds = 450)
    {
        if (compositionVisual == null)
            return;

        var compositor = compositionVisual.Compositor;
        var animationGroup = compositor.CreateAnimationGroup();

        var sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        
        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        animationGroup.Add(sizeAnimation);
        animationGroup.Add(offsetAnimation);

        var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Size"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;
        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }


    public static void MakeSizeOpacityAnimated(CompositionVisual? compositionVisual, double milliseconds = 450)
    {
        if (compositionVisual == null)
            return;

        var compositor = compositionVisual.Compositor;
        var animationGroup = compositor.CreateAnimationGroup();

        var sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        
        var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = "Opacity";
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        opacityAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        
        animationGroup.Add(sizeAnimation);
        animationGroup.Add(opacityAnimation);
        animationGroup.Add(offsetAnimation);

        var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Size"] = animationGroup;
        implicitAnimationCollection["Opacity"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;
        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }
}