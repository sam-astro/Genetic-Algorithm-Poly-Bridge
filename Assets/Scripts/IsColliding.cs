using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class IsColliding : MonoBehaviour
{
	public bool isColliding;
	public bool failed;

	public string tagName;

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == tagName)
		{
			isColliding = true;
		}
		else if (collision.gameObject.tag == "danger")
		{
			isColliding = true;
			failed = true;
		}
	}

	void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.tag == tagName)
		{
			isColliding = false;
		}
	}
}