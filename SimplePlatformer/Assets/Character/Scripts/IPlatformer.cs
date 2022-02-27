using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPlatformer
{
    /// <summary> Will adjust the position according to the velocity, input and booleans listed. </summary>
    public void Move(Vector2 input, bool jump, bool climb, bool jumpCont, bool dash);

    /// <summary> Returns the current energy of the controller 0 to 1. </summary>
    public float Energy { get; }
    /// <summary> Returns the current number of dashes availible </summary>
    public int Dashes { get; }
    /// <summary> Returns the current velocity of the character </summary>
    public Vector2 Velocity { get; }
    /// <summary> Returns whether the character is on the ground </summary>
    public bool TouchingGround { get; }
    /// <summary> Returns whether the character is touching a wall from either side </summary>
    public bool TouchingWall { get; }

    public bool Climbing { get; }

    /// <summary> Called when the player dashes </summary>
    public UnityEvent OnDash { get; }
    public UnityEvent OnJump { get; }

    /// <summary> Set Energy to full, Fill All Dashes. </summary>
    public void RechargeAll();
}
