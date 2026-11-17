using UnityEngine;

public class PaintManager : MonoBehaviour
{
    public Texture2D texture;        //Starting image.
	public Texture2D stampTexture;   //Texture to Graphics.Drawtexture on my RenderTexture.
	public float posX = 256f;        //Position the DrawTexture command while testing.
	public float posY = 256f;        //Position the DrawTexture command while testing.
	RenderTexture rt;                //RenderTexture to use as buffer.
	[SerializeField] private Texture2D splatterOne, splatterTwo, splatterThree, splatterFour, splatterFive;

	void Start ()
	{
		rt = new RenderTexture (512, 512, 32);           //Create RenderTexture 512x512 pixels in size.
		GetComponent<MeshRenderer>().material.SetTexture ("_BaseMap", rt);   //Assign my RenderTexure to be the main texture of my object.
		Graphics.Blit (texture, rt);                     //Blit my starting texture to my RenderTexture.
	}

	void Update ()
	{
        Debug.Log(rt);
		RenderTexture.active = rt;                      //Set my RenderTexture active so DrawTexture will draw to it.
		GL.PushMatrix ();								//Saves both projection and modelview matrices to the matrix stack.
		GL.LoadPixelMatrix (0, 512, 512, 0);			//Setup a matrix for pixel-correct rendering.
		//Draw my stampTexture on my RenderTexture positioned by posX and posY.
		Graphics.DrawTexture (new Rect (posX - stampTexture.width / 2, (rt.height - posY) - stampTexture.height / 2, stampTexture.width, stampTexture.height), stampTexture);
		GL.PopMatrix ();								//Restores both projection and modelview matrices off the top of the matrix stack.
		RenderTexture.active = null;                    //De-activate my RenderTexture.
	}
}
