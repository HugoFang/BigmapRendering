// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;

/// \ingroup Demo
/// Implements a standard spectator/fly camera.
/// 
/// Simple class adds movement to the FP Controller base. Useful for 
/// debug cameras and the like.
[AddComponentMenu("SECTR/Demos/SECTR Ghost Controller")]
public class SECTR_GhostController : SECTR_FPController 
{
	#region Public Interface
	public float FlySpeed = 0.5f;
	public float AccelerationRatio = 1f;
	public float SlowDownRatio = 0.5F;
    #endregion

	#region Unity Interface	
	protected override void Update()
	{
		base.Update();


		if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
	    {
	        FlySpeed *= AccelerationRatio * Time.deltaTime;
	    }
	
	    if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
	    {
			FlySpeed /= AccelerationRatio * Time.deltaTime;
	    }
	
	    if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
	    {
			FlySpeed *= SlowDownRatio * Time.deltaTime;
	    }
	
	    if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
	    {
			FlySpeed /= SlowDownRatio * Time.deltaTime;
	    }

		Vector2 vJoystick;
		if(Input.multiTouchEnabled && !Application.isEditor)
		{
			vJoystick = GetScreenJoystick(false);
		}
		else
		{
			vJoystick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		}
        float lerpT = transform.position.y / 1000f;
        float speed = Mathf.Lerp(20f, FlySpeed, lerpT);
		transform.position += (transform.forward * speed * Time.deltaTime * vJoystick.y) + (transform.right * speed * Time.deltaTime * vJoystick.x);
	
	    if (Input.GetKey(KeyCode.E))
	    {
			transform.position += transform.up * FlySpeed * Time.deltaTime * 0.5F;
	    }
	    else if (Input.GetKey(KeyCode.Q))
	    {
			transform.position -= transform.right * FlySpeed * Time.deltaTime * 0.5F;
	    }
	}
	#endregion
}