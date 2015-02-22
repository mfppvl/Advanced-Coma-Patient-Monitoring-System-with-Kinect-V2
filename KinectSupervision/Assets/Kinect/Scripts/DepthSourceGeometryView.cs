using UnityEngine;
using System.Collections;

public class DepthSourceGeometryView : MonoBehaviour {
	public GameObject DepthSourceManager;

	public Material Material;

	private DepthSourceManager _DepthManager;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (DepthSourceManager == null)
		{
			return;
		}
		
		_DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
		if (_DepthManager == null)
		{
			return;
		}	
	}

	void OnRenderObject() 
	{
		Material.mainTexture = _DepthManager.GetDepthTexture ();
		Material.SetPass(0);

		if (Camera.current.name == "Depth Camera") 
		{
			Graphics.DrawProcedural (MeshTopology.Points, _DepthManager.GetDepthWidth () * _DepthManager.GetDepthHeight (), 1);
		}
	}
}