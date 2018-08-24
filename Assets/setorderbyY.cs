using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class setorderbyY : MonoBehaviour
{
	private SpriteRenderer _sprite;

	private void Awake()
	{
		_sprite = GetComponent<SpriteRenderer>();
	}

	void Update ()
	{
		_sprite.sortingOrder = (int)-transform.position.y;
	}
}
