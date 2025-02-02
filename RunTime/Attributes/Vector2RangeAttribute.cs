using System;
using UnityEngine;

namespace IslandGenerator
{
    /// <summary>
    /// <b> Description : </b>
    /// 
    /// <para> A custom property attribute to <b> define a range for a Vector2 field </b> in the Unity Inspector. </para> 
    /// <para> And also, <b> show a interactable graph of the Vector2 </b>, you can set value by clicking or holding left clic on it. </para>
    /// 
    /// <para> --------- </para> 
    /// 
    /// <b> Usage example : </b>
    /// <code>
    /// [Vector2Range(0f, 0f, 512f, 666f)]
    /// public Vector2 MapSizeInMeters;
    /// </code>
    /// 
    /// <para> --------- </para> 
    /// 
    /// <b> Important notes : </b> 
    /// <para> <b> 1. </b> Unity does not support Vector2 as a parameter for attributes, which is why the there is no constructor using two Vector2. </para>
    /// 
    /// 
    /// <b> 2. </b> If you want to pass a variable's value as a parameter, you need to past a 'const (int/float)' variable. 
    /// 
    /// <para> If the variable you want to use is not 'const' you can't create a copy of your variable and add 'const' to it. </para>
    /// 
    /// <para> <b> Code exemple of what you CAN'T do : </b> 
    /// 
    /// <code> 
    /// public Vector2 MapSizeInMeters = new(512, 666);
    /// 
    /// // ERROR : A field initializer cannot reference the non-static field, method, or property 'name'.
    /// const Vector2 MAP_SIZE_IN_METERS = MapSizeInMeters;
    /// </code>  
    /// </para> </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class Vector2RangeAttribute : PropertyAttribute
    {
        public Vector2 Min { get; }
        public Vector2 Max { get; }


        public Vector2RangeAttribute(float p_minimumX, float p_minimumY, float p_maximumX, float p_maximumY)
        {
            Min = new Vector2(p_minimumX, p_minimumY);
            Max = new Vector2(p_maximumX, p_maximumY);
        }

        // NOTE : Unity doesn't support Vector2 parameter for Attributes, this is why it's commented.
        //public Vector2RangeAttribute(Vector2 p_minimums, Vector2 p_maximums)
        //{
        //    Min = p_minimums;
        //    Max = p_maximums;
        //}
    }
}