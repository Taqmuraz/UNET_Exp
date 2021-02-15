using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class Player : NetworkBehaviour
{
	class VectorMessage : MessageBase
	{
		public Vector3 vector { get; private set; }

		public VectorMessage(Vector3 vector)
		{
			this.vector = vector;
		}
		public VectorMessage()
		{
		}

		public override void Deserialize(NetworkReader reader)
		{
			vector = reader.ReadVector3();
		}
		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(vector);
		}
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		Debug.Log("OnStartLocalPlayer");
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		Debug.Log("OnStartAuthority");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		Debug.Log("OnStartServer");

		Vector3 pos = Random.insideUnitSphere * 3f;
		pos.y = 0f;
		transform.position = pos;

		NetworkServer.RegisterHandler(id, ChangeColorMessage);
	}

	short id
	{
		get
		{
			return (short)(1000 + netId.Value);
		}
	}


	void ChangeColorMessage(NetworkMessage netMsg)
	{
		ChangeColor(netMsg.ReadMessage<VectorMessage>().vector);
	}

	void Start()
	{
		ChangeColor(isLocalPlayer ? Vector3.forward : Vector3.right);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		Debug.Log("OnStartClient");

		NetworkManager.singleton.client.RegisterHandler(id, ChangeColorMessage);
	}

	void ChangeColor(Vector3 color)
	{
		GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);
	}

	void OnMouseDown()
	{
		Debug.Log("click");
		var msg = new VectorMessage((Random.onUnitSphere + Vector3.one) * 0.5f);
		
		if (isServer)
		{
			foreach (var v in NetworkServer.connections) v.Send(id, msg);
		}
		if (isClient)
		{
			NetworkManager.singleton.client.Send(id, msg);
			ChangeColor(msg.vector);
		}
	}
}
