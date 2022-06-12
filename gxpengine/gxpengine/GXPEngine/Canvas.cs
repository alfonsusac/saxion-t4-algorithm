using System;
using System.Drawing;
using GXPEngine.Core;

namespace GXPEngine
{
	/// <summary>
	/// The Canvas object can be used for drawing 2D visuals at runtime.
	/// </summary>
	public class Canvas : Sprite
	{
		protected Graphics _graphics;
		protected bool _invalidate = false;

		//------------------------------------------------------------------------------------------------------------------------
		//														Canvas()
		//------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the Canvas class.
		/// It is a regular GameObject that can be added to any displaylist via commands such as AddChild.
		/// It contains a <a href="http://msdn.microsoft.com/en-us/library/system.drawing.graphics(v=vs.110).aspx">System.Drawing.Graphics</a> component.
		/// </summary>
		/// <param name='width'>
		/// Width of the canvas in pixels.
		/// </param>
		/// <param name='height'>
		/// Height of the canvas in pixels.
		/// </param>
		public Canvas (int width, int height) : this(new Bitmap (width, height))
		{
			name = width + "x" + height;
		}

		public Canvas (System.Drawing.Bitmap bitmap) : base (bitmap)
		{
			_graphics = Graphics.FromImage(bitmap);
			_invalidate = true;
		}

		public Canvas(string filename):base(filename)
		{
			_graphics = Graphics.FromImage(texture.bitmap);
			_invalidate = true;
		}


		//------------------------------------------------------------------------------------------------------------------------
		//														graphics
		//------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the graphics component. This interface provides tools to draw on the sprite.
		/// See: <a href="http://msdn.microsoft.com/en-us/library/system.drawing.graphics(v=vs.110).aspx">System.Drawing.Graphics</a>
		/// </summary>
		public Graphics graphics {
			get { 
				_invalidate = true;
				return _graphics; 
			}
		}

		//------------------------------------------------------------------------------------------------------------------------
		//														Render()
		//------------------------------------------------------------------------------------------------------------------------
		override protected void RenderSelf(GLContext glContext) {
			if (_invalidate) {
				_texture.UpdateGLTexture ();
				_invalidate = false;
			}

			base.RenderSelf (glContext);
		}

		//------------------------------------------------------------------------------------------------------------------------
		//														alpha
		//------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Draws a Sprite onto this Canvas.
		/// It will ignore Sprite properties, such as color and animation.
		/// </summary>
		/// <param name='sprite'>
		/// The Sprite that should be drawn.
		/// </param>
		private PointF[] destPoints = new PointF[3];
		public void DrawSprite(Sprite sprite) {
			float halfWidth = sprite.texture.width / 2.0f;
			float halfHeight = sprite.texture.height / 2.0f;
			Vector2 p0 = sprite.TransformPoint(-halfWidth, -halfHeight);
			Vector2 p1 = sprite.TransformPoint(halfWidth, -halfHeight);
			Vector2 p2 = sprite.TransformPoint(-halfWidth, halfHeight);
			destPoints[0] = new PointF(p0.x, p0.y);
			destPoints[1] = new PointF(p1.x, p1.y);
			destPoints[2] = new PointF(p2.x, p2.y);
			graphics.DrawImage(sprite.texture.bitmap, destPoints);
		}

		public void DrawSprite2(Sprite sprite)
		{
			float width = Math.Abs(sprite.width / sprite.scaleX);
			float height = Math.Abs(sprite.height / sprite.scaleY);

			Vector2[] points = sprite.GetExtents();
			destPoints[0] = new PointF(points[0].x, points[0].y);
			destPoints[1] = new PointF(points[1].x, points[1].y);
			destPoints[2] = new PointF(points[3].x, points[3].y);
			//System.Drawing.Rectangle sourceRect = new System.Drawing.Rectangle(0, 0, (int)width, (int)height);

			float[] uvs = sprite.GetUVs();
			System.Drawing.Rectangle sourceRect = new System.Drawing.Rectangle((int)(uvs[0] * sprite.texture.width), (int)(uvs[1] * sprite.texture.height), (int)width, (int)height);
			graphics.DrawImage(sprite.texture.bitmap, destPoints, sourceRect, GraphicsUnit.Pixel);
		}


		// Called by the garbage collector
		~Canvas() {
			_graphics.Dispose();
			
		}
	}
}

