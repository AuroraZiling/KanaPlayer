using System;
using Avalonia.Animation.Easings;

namespace KanaPlayer.Helpers.Animations;

public enum EasingIntensity
{
    Soft,
    Normal,
    Strong
}

public class KanaEaseInBackOutBack : Easing
{
    public EasingIntensity BounceIntensity { get; set; } = EasingIntensity.Normal;

    public override double Ease(double progress)
    {
        var c1 = BounceIntensity switch
        {
            EasingIntensity.Soft   => 0.9,
            EasingIntensity.Normal => 1.15,
            EasingIntensity.Strong => 1.5,
            _                      => 1.0
        };

        var c2 = c1 * 1.525;
        if (progress < 0.5)
        {
            var term = 2 * progress;
            return Math.Pow(term, 2) * ((c2 + 1) * term - c2) / 2.0;
        }
        else
        {
            var term = 2 * progress - 2;
            return (Math.Pow(term, 2) * ((c2 + 1) * term + c2) + 2) / 2.0;
        }
    }
}

public class KanaEaseInOutBack: Easing
{
    public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
        
    public override double Ease(double progress)
    {
        var c1 = BounceIntensity switch
        {
            EasingIntensity.Soft   => 0.9,
            EasingIntensity.Normal => 1.15,
            EasingIntensity.Strong => 1.5,
            _                      => 1.0
        };
            
        var c3 = c1 + 1;
        var smoothedStart = progress * progress * (2 - progress);

        return 1 + c3 * Math.Pow(smoothedStart - 1, 3) + c1 * Math.Pow(smoothedStart - 1, 2);
    }

}
    
public class KanaEaseOutBack: Easing
{
    public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
     
    public override double Ease(double progress)
    {
        var c1 = BounceIntensity switch
        {
            EasingIntensity.Soft   => 0.9,
            EasingIntensity.Normal => 1.15,
            EasingIntensity.Strong => 1.5,
            _                      => 1.0
        };
        
        var c3 = c1 + 1;
        return 1 + c3 * Math.Pow(progress - 1, 3) + c1 * Math.Pow(progress - 1, 2);
    }
}
    
public class KanaEaseOut : Easing
{
    public override double Ease(double progress)
    {
        var warpedProgress = Math.Sqrt(progress);
        return 1.0 - Math.Pow(1.0 - warpedProgress, 3);
    }
}
    
public class KanaEaseInOut : Easing
{
    public override double Ease(double progress)
    {
        var warpedProgress = Math.Sqrt(progress);
        
        if (warpedProgress < 0.5)
        {
            return 4.0 * warpedProgress * warpedProgress * warpedProgress;
        }
        var factor = -2.0 * warpedProgress + 2.0;
        return 1.0 - Math.Pow(factor, 3) / 2.0;
    }
}