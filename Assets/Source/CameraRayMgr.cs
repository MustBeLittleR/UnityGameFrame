using System;
using UnityEngine.EventSystems;
using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;

namespace WCG
{
	public class CameraRayMgr
	{
		public static readonly CameraRayMgr Instance = new CameraRayMgr();

		public CameraRayMgr ()
		{}

		public static void ScreenPointToRay(Camera camera, Vector3 v3RayDir, LuaFunction backfun)
		{
			if (camera == null || IsPointerOverGameObject())
				return;

			Ray r = camera.ScreenPointToRay(v3RayDir);
			RaycastHit h;
			if (Physics.Raycast(r, out h, 500, camera.cullingMask))
			{
				if (h.collider != null && backfun != null)
				{
					backfun.Call(h.collider.gameObject, h.point, h.collider.gameObject.tag);
				}
			}
		}

		static int SortByDistance(RaycastHit hitA, RaycastHit hitB)
		{
			return hitA.distance < hitB.distance ? -1 : 1;
		}

		public static void ScreenPointToRayEx(Camera camera, Vector3 v3RayDir, LuaFunction backfun, int exceptInsID, bool bIgnUI = false)
		{
			if (camera == null || (!bIgnUI && IsPointerOverGameObject()))
				return;

			Ray r = camera.ScreenPointToRay(v3RayDir);
			RaycastHit[] array = Physics.RaycastAll(r, 1000, camera.cullingMask);
			Array.Sort(array, SortByDistance);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit hit = array[i];
				GameObject goObj = hit.collider.gameObject;
				if (exceptInsID != goObj.GetInstanceID())
				{
					backfun.Call(goObj, hit.point, goObj.tag);
					break;
				}
			}
		}

		public static bool IsPointerOverGameObject(int touchIndex = 0)
		{
			if (Input.touchCount > 0)
			{
				if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(touchIndex).fingerId))
					return true;
				else
					return false;
			}
			return EventSystem.current.IsPointerOverGameObject();
		}
	}
}

