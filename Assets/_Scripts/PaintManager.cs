using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{
	[SerializeField] private PlayerMovement playerMovement;
	[SerializeField] private Transform playerPos;
	[SerializeField] private Rigidbody2D rb;
    [SerializeField] Texture2D texture;        //Starting image.
	private RenderTexture rt;                //RenderTexture to use as buffer.
	[SerializeField] private List<Texture2D> splatlist = new List<Texture2D>();
	[SerializeField] private Texture2D lastSplat, stampTexture;
	[SerializeField] private MeshRenderer meshRenderer;
	private int i;
	private float playerSpeed;
	[SerializeField] private Texture2D splatterOne, splatterTwo, splatterThree, splatterFour, splatterFive;

	void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		rt = new RenderTexture (1024, 1024, 32);  
		rt.Create();         //Create RenderTexture 512x512 pixels in size.
		GetComponent<MeshRenderer>().material.SetTexture ("_BaseMap", rt);   //Assign my RenderTexure to be the main texture of my object.
		Graphics.Blit (texture, rt);                     //Blit my starting texture to my RenderTexture.
		splatlist.Add(splatterOne);
		splatlist.Add(splatterTwo);
		splatlist.Add(splatterThree);
		splatlist.Add(splatterFour);
		splatlist.Add(splatterFive);
	}

	private Texture2D randomSplatter()
	{
		if (lastSplat != null)
		{
			splatlist.Remove(lastSplat);
		}
		i = Random.Range(0, splatlist.Count);
		if (lastSplat != null)
		{
			splatlist.Add(lastSplat);
		}
		lastSplat = splatlist[i];
		return lastSplat;
	}

    void LateUpdate()
    {
		if (rb.linearVelocity.magnitude >= 0f || playerMovement.IsGrappleHeld())
		{
			{
				if (playerMovement.IsGrappleHeld())
				{
					playerSpeed = playerMovement.currentSpeed;
					
				}
				else
				{
					playerSpeed = rb.linearVelocity.magnitude;
				}
			}
			CreateSplatter();
    	}
	}

    public void CreateSplatter()
	{
		Bounds mapBounds = meshRenderer.bounds;
		
		float quadX = Mathf.InverseLerp(mapBounds.min.x, mapBounds.max.x, playerPos.position.x);
		float quadY = Mathf.InverseLerp(mapBounds.min.y, mapBounds.max.y, playerPos.position.y);
		if (quadX <= 0f || quadX >= 1f || quadY <= 0f || quadY >= 1f)
		{
			return;
		}
		float quadPosX = quadX * rt.width;
		float quadPosY = (1f - quadY) * rt.height; 

		stampTexture = splatterOne;

		float quadWidth = (stampTexture.width * playerSpeed/2) / mapBounds.size.x;
		float quadHeight = (stampTexture.height * playerSpeed/2) / mapBounds.size.y;

		float rotationAngle = Random.Range(0f, 360f);


		RenderTexture.active = rt;                      //Set my RenderTexture active so DrawTexture will draw to it.
		GL.PushMatrix ();								//Saves both projection and modelview matrices to the matrix stack.
		GL.LoadPixelMatrix (0, 1024, 1024, 0);			//Setup a matrix for pixel-correct rendering.
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(new Vector3(quadPosX, quadPosY, 0f), Quaternion.Euler(0f, 0f, rotationAngle), Vector3.one);
		GL.MultMatrix(rotationMatrix);
		
		Graphics.DrawTexture (new Rect (-quadWidth / 2f, -quadHeight / 2f, quadWidth, quadHeight), stampTexture);
		GL.PopMatrix ();								//Restores both projection and modelview matrices off the top of the matrix stack.
		RenderTexture.active = null;                    //De-activate my RenderTexture.
	}
}
