using System;
using System.Collections.Generic;
using System.Drawing;

using System.Windows.Forms;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX;

using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;

using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;
using GdiSolidBrush = System.Drawing.SolidBrush;
using GdiTextureBrush = System.Drawing.TextureBrush;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using RectangleF = SharpDX.Mathematics.Interop.RawRectangleF;
using RawRectangleF = SharpDX.Mathematics.Interop.RawRectangleF;
using RawColor = SharpDX.Mathematics.Interop.RawColor4;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

using MATRIZES;

namespace myControlsLibrary.Renderizacao
{
    public class MyDevice2D
    {
        /// classe MyDevice2D: encapsula e provê métodos simples para dispositivo de desenho do Direct2D.
        /// 
        /// métodos públicos:
        /// 
        /// static Instance():  MyDevice2D.  Padrão de projeto Singleton.
        /// BeginDraw(controlContainer: Control):void  permite a renderização em lotes de MyImages.
        /// EndDraw(controlContainer: Control ): void  permite a renderização em lotes de MyImages.
        /// Clear(container: Control, cor: Color): void. preenche o Control com uma cor.
        /// 
        /// Flush(controlContainer: Control): void  renderiza qualquer desenho pendente.
        /// FillColorDevice(container: Control, cor: Color): void preenche o control  container  com uma cor.
        /// FillColorDevice(container: Control, cor:Color ,rectFill: Rectangle):void preenche uma área do control container com uma cor.
        /// 

        private static SharpDX.Direct2D1.Factory factory = new SharpDX.Direct2D1.Factory();

      
        internal static SharpDX.DirectWrite.Factory FactoryDirectWrite = new SharpDX.DirectWrite.Factory();


        internal static Dictionary<IntPtr, RenderTarget> render = new Dictionary<IntPtr, RenderTarget>();

        private static MyDevice2D direct2D;
        public static MyDevice2D Instance()
        {
            if (direct2D == null)
                direct2D = new MyDevice2D();
            return direct2D;

          
        }
        private void InitRender(Control container)
        {
            // Direct2D
            var renderTargetProperties = new RenderTargetProperties()
            {
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore)
            };
            var hwndRenderTargetProperties = new HwndRenderTargetProperties()
            {
                Hwnd = container.Handle,
                PixelSize = new Size2(container.Width, container.Height),
                PresentOptions = PresentOptions.Immediately,
            };
            var renderTarget = new WindowRenderTarget(factory, renderTargetProperties, hwndRenderTargetProperties);
            render[container.Handle] = renderTarget;
        }


        public void BeginDraw(Control container)
        {
            if (container == null)
                return;
            RegisterRenderTarget(container);
            render[container.Handle].BeginDraw();
        } // BeginDraw()

        public void EndDraw(Control container)
        {
            if (container == null)
                return;
            RegisterRenderTarget(container);
            render[container.Handle].EndDraw();
        }
        
        public  void Clear(Control container, Color cor)
        {
            if (container == null)
                return;
            RegisterRenderTarget(container);
            render[container.Handle].Clear(new SharpDX.Mathematics.Interop.RawColor4(cor.R, cor.G, cor.B, cor.A));
        }

        internal void RegisterRenderTarget(Control container)
        {
            if (container == null)
                return;
            RenderTarget rndrTarget = null;
            MyDevice2D.render.TryGetValue(container.Handle, out rndrTarget);
            if (rndrTarget == null)
                MyDevice2D.Instance().InitRender(container);
        }

    } // class MyDevice2D()

    public class MyImage
    {
        /// Propriedades:
        /// texture: SlimDX.Direct2D.Bitmap --> imagem para desenhar no Direct2D.
        /// textureGDI: private System.Drawing.Bitmap --->imagem para base de arquivos de imagem, e de formação da imagem Direct2D.
        /// 
        /// Construtor:
        /// MyImage(fileNameImage:string ,controlToDraw: Control,sizeImage:  Size )
        /// MyImage(imagemGDI: Bitmap ,controlToDraw:Control, szImage: Size )
        /// 
        /// Métodos:
        /// GetGDIBitmap(): obtem a imagem GDI.
        /// GetDimensions(): otém as dimensõoes da imagem,para deseho,e dimensões do novo MySprite();
        ///                                                
        /// void Resize(nwSize:Size): redimensiona a imagem.                           
        /// 
        /// void Draw(location: vetor2): desenha a imagem no ponto de entrada, dentro do control container definido no construtor da classe.
        /// void Draw(location: vetor2, opacity: float).
        /// void Draw(location: vetor2,rectSource: Rectangle ,rectDestiny: Rectangle):desenha uma porção da imagem numa área de destino.
        /// void Draw(texto: string, location: vetor2 , sizeText: SizeF, colorText:  Color, fontFamily: string, szFont: float)
        /// 
        /// void Resize(nwSize: Size).
        /// void Rotate(angleInGrauss:float): seta o ângulo de rotação da imagem quando desenhada.
        /// 
        /// void Begin(): começa a renderização do desenho. A imagem não faz parte de um lote de imagens a serem desenhadas num mesmo Control container.
        /// void End(): termina a renderização do desenho.  A imagem não faz parte de um lote de imagens a serem desenhadas num mesmo Control container.
        /// void Dispose(): libera os recursos, como as imagens GDI e Direct 2D.
        /// void Clear(cor: Color): preenche o control da imagem com uma cor.

        public Bitmap imageDirect2D;
        private System.Drawing.Bitmap imageGDI;
        public IntPtr idRender;
        public SizeF size { get; set; }

        private static int countImages = 1;

        internal int idImage = countImages++;

        public readonly static RawMatrix3x2 Identity = new RawMatrix3x2(1, 0, 0, 1, 0, 0);


        public static void Translation(float x, float y, out RawMatrix3x2 result)
        {
            result = Identity;
            result.M31 = x;
            result.M32 = y;
        }

        private void SetMatrixIdentity()
        {
            MyDevice2D.render[this.idRender].Transform = Identity; // volta a matriz identidade para a matriz de transformação.
        }

        private void SetMatrixTranslation(vetor2 location)
        {
            RawMatrix3x2 m_Translation; // cria a matriz que servirá de translação.
            Translation((float)location.X, (float)location.Y, out m_Translation); // cria a matriz de translacao.
            MyDevice2D.render[this.idRender].Transform = m_Translation; // seta a matriz do RenderTarget para a matriz de translação.
        }


        public MyImage(System.Drawing.Bitmap image, Control container, Size size)
        {
            if ((container == null) || (image == null))
                return;

            this.imageGDI = new System.Drawing.Bitmap(image, size);
            this.idRender = container.Handle;
            this.size = new SizeF(size.Width, size.Height);

            MyDevice2D.Instance().RegisterRenderTarget(container);

            this.imageDirect2D = GetDirect2DBitmapWithSharpDX(MyDevice2D.render[container.Handle], this.imageGDI);
            this.idRender = container.Handle;
        } // MyImage()


        public MyImage(string fileNameImage, Control container, Size size)
        {

            if ((container == null) || (fileNameImage == null))
                return;

            this.imageGDI = new System.Drawing.Bitmap(fileNameImage);
            this.imageGDI = new System.Drawing.Bitmap(this.imageGDI, size);

            this.size = new SizeF(size.Width, size.Height);
            this.idRender = container.Handle;

            MyDevice2D.Instance().RegisterRenderTarget(container);

            this.imageDirect2D = GetDirect2DBitmapWithSharpDX(MyDevice2D.render[container.Handle], this.imageGDI);
        } // MyImage()

        /// <summary>
        /// desenha um texto. Há que ter os métodos Begin() e End() para efetivar a renderização.
        /// </summary>
        public void Draw(vetor2 location)
        {
            SetMatrixTranslation(location);

            // desenha o Direct2D1.Bitmap, com a matriz de translação setada na matriz de transformação do RenderTarget.
            MyDevice2D.render[this.idRender].DrawBitmap(this.imageDirect2D, 1, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
            SetMatrixIdentity();
        } // Draw()

        
        /// <summary>
        /// desenha um texto. Há que ter os métodos Begin() e End() para efetivar a renderização.
        /// </summary>
        public void Draw(vetor2 location, float opacity)
        {
            SetMatrixTranslation(location);
            MyDevice2D.render[this.idRender].DrawBitmap(this.imageDirect2D, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
            SetMatrixIdentity();
        } // Draw()

        /// <summary>
        /// desenha um texto. Há que ter os métodos Begin() e End() para efetivar a renderização.
        /// </summary>
        public void Draw(vetor2 location, float opacity, RawRectangleF rectSource)
        {
            
            SetMatrixTranslation(location); // a coisa mais certa é setar a matriz transform do RenderTarget, e depois setar a transform para Identity.

            MyDevice2D.render[this.idRender].DrawBitmap(this.imageDirect2D, 1.0f, BitmapInterpolationMode.Linear, rectSource);

            SetMatrixIdentity();// restaura a matriz identidade , para a matriz transform do RenderTarget.
        } // Draw()


        /// <summary>
        /// desenha um texto. Há que ter os métodos Begin() e End() para efetivar a renderização.
        /// </summary>
        public void Draw(string texto, RectangleF rectDEstiny, Color cor, Font fonte)
        {

            // a coisa mais certa é setar a matriz transform do RenderTarget, e depois setar a transform para Identity.
            SetMatrixTranslation(new vetor2( rectDEstiny.Left, rectDEstiny.Top)); 

            rectDEstiny.Left = 0;
            rectDEstiny.Top = 0;

            SharpDX.DirectWrite.TextFormat format = new SharpDX.DirectWrite.TextFormat(MyDevice2D.FactoryDirectWrite, "Arial", fonte.Size); // "formato" do texto a ser desenhado.
            
            SolidColorBrush brush = new SolidColorBrush(  MyDevice2D.render[this.idRender], new RawColor(cor.R, cor.G, cor.B, cor.A)); // "brush" para o desenho.


            MyDevice2D.render[this.idRender].DrawText( texto, format, rectDEstiny, brush); // desenha o texto


            SetMatrixIdentity();// restaura a matriz identidade , para a matriz transform do RenderTarget.

        } // DrawText()


        /// <summary>
        /// utilizado antes da imagem a ser desenhada.
        /// </summary>
        public void Begin()
        {
            MyDevice2D.render[this.idRender].BeginDraw();
        } // Begin()

        /// <summary>
        /// utilizado depois da imagem a ser desenhada.
        /// </summary>
        public void End()
        {
            MyDevice2D.render[this.idRender].EndDraw();
        } // End()

        /// <summary>
        /// preenche com uma cor, o control associado ao renderizador Direct2D.
        /// </summary>
        /// <param name="cor">cor a ser preenchido o control.</param>
        public void Clear(Color cor)
        {
            MyDevice2D.render[this.idRender].Clear(new RawColor(cor.R, cor.G, cor.B, cor.A));
        } // Clear()


        /// <summary>
        /// rotaciona a imagem em um incremento de ângulo, em graus.
        /// </summary>
        /// <param name="incAngulo">incremento de ângulo para rotacionar.</param>
        public void Rotate(double incAngulo)
        {
            this.imageGDI = RotateImage(this.imageGDI, (float)incAngulo);
            this.imageDirect2D = GetDirect2DBitmapWithSharpDX(MyDevice2D.render[this.idRender], this.imageGDI);

        } // Rotate()


        private System.Drawing.Bitmap RotateImage(System.Drawing.Bitmap bmp, float angle)
        {
            System.Drawing.Bitmap rotatedImage = new System.Drawing.Bitmap(bmp.Width, bmp.Height);
            rotatedImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                // Rotate
                g.RotateTransform(angle);
                // Restore rotation point in the matrix
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                // Draw the image on the bitmap
                g.DrawImage(bmp, new Point(0, 0));
            }
            return rotatedImage;
        } // RotateImage()

        public void Resize(Size newSize)
        {
            this.imageGDI = new System.Drawing.Bitmap(this.imageGDI, newSize);
            this.imageDirect2D = GetDirect2DBitmapWithSharpDX(MyDevice2D.render[this.idRender], this.imageGDI);

            this.size = new SizeF(newSize.Width, newSize.Height);
        } // Resize()



        internal static Bitmap GetDirect2DBitmapWithSharpDX(RenderTarget rt, System.Drawing.Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (image.PixelFormat != GdiPixelFormat.Format32bppArgb)
                return null;

            var imageData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                                           System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);


            var dataStream = new DataStream(imageData.Scan0, imageData.Stride * imageData.Height, true, false);
            var properties = new BitmapProperties
            {
                PixelFormat = new SharpDX.Direct2D1.PixelFormat
                {
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    AlphaMode = AlphaMode.Premultiplied
                }
            };

      
            //Load the image from the gdi resource
            var bitmapDirect2D = new Bitmap(rt, new Size2(image.Width, image.Height), dataStream, imageData.Stride, properties);

            image.UnlockBits(imageData);

            return bitmapDirect2D;
        } // GetDirect2DBitmapWithSharpDX()
    
        public System.Drawing.Bitmap GetGDIBitmap()
        {
            return imageGDI;
        }
        public Size GetDimensions()
        {
            return new Size((int)this.size.Width, (int)this.size.Height);
        }

        public void Dispose()
        {
            this.imageGDI.Dispose();
            this.imageDirect2D.Dispose();
        } //Dispose()

    }

    public class RenderScene
    {
        /// Classe MySceneImage: classe especializada em desenhar as imagens num buffer de imagens.
        /// Métodos:
        ///     AddImage(image: MyImage): void. Adiciona uma imagem na lista de imagen para desenhar.
        ///     RemoveImage(image: MyImage): void. remove uma imagem da lista de imagens para desenhar.
        ///     void DrawScene(). desenha a imagem da cena inteira. Em seguida, limpa as imagens e localizações, para um próximo frame de renderização.
        ///



        public List<MyImage> imagesD2D;
        public List<vetor2> locations;
        private List<float> opacities;
        private Control container;
        private Size sizeScene;
        private vetor2 locationScene;
        private MyImage sceneImageD2D;

        System.Drawing.Bitmap sceneBitmapGDI;

        public RenderScene(Control container, vetor2 locationScene, Size szScene)
        {
            this.sizeScene = szScene;
            this.container = container;
            this.locationScene = locationScene;
            this.imagesD2D = new List<MyImage>();
            this.locations = new List<MATRIZES.vetor2>();
            this.opacities = new List<float>();
            this.sceneBitmapGDI = new System.Drawing.Bitmap(szScene.Width,szScene.Height); // cria uma imagem GDI da cena.
        } // MySceneImage()


        public void AddImage(MyImage image, vetor2 location, float opacity)
        {
            if ((this.imagesD2D == null) || (this.imagesD2D.Count == 0))
                this.imagesD2D = new List<MyImage>();
            if ((this.locations == null) || (this.locations.Count == 0))
                this.locations = new List<vetor2>();
            this.imagesD2D.Add(image);
            this.locations.Add(location);
            this.opacities.Add(opacity);
        } // AddImage()


        public void RemoveImage(MyImage image)
        {
            int index = this.imagesD2D.FindIndex(k => k.idImage == image.idImage);
            if (index != -1)
            {
                this.imagesD2D.RemoveAt(index);
                this.locations.RemoveAt(index);
            } // if
        } // RemoveImage()

        // desenha numa só imagem, todas imagens registradas. Os dados precisam ser inseridos a cada renderização de frame.
        // Não apaga o frame de renderização anterior.
        public void DrawScene()
        {

            this.sceneBitmapGDI = new System.Drawing.Bitmap(sizeScene.Width, sizeScene.Height); // inicializa a imagem total da cena a renderizar.

            Graphics g = Graphics.FromImage(sceneBitmapGDI); // prepara para desenhar imagens GDI para dentro da imagem GDI da cena.
         
            for (int x = 0; x < this.imagesD2D.Count; x++)
            {
                // desenha a imagem GDI currente para dentro da imagem GDI da cena.
                g.DrawImageUnscaled(this.imagesD2D[x].GetGDIBitmap(), new Point((int)this.locations[x].X, (int)this.locations[x].Y));

            } // for x
            this.sceneImageD2D = new MyImage(sceneBitmapGDI, container, sizeScene); // obtem a imagem Direct2D da cena inteira.

            // desenha a cena inteira, todas imagens foram resumidas em uma só imagem D2D.
            sceneImageD2D.Begin();
            sceneImageD2D.Draw(locationScene); // desenha o frame de renderização.
            sceneImageD2D.End();

            // limpa as listas, para construir uma nova cena de buffer.
            this.imagesD2D.Clear();
            this.locations.Clear();
            this.sceneImageD2D.Dispose();
            g.Dispose();

        } // DrawScene()

    } // class RenderScene

} // namespace
