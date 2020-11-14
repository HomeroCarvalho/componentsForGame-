using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MATRIZES;
using myControlsLibrary.Renderizacao;
using System.Windows.Forms;
using Utils;

namespace myControlsLibrary.Components
{
    /// <summary>
    /// classe para compor um joystick sob o controle de um vetor direção, que pode servir para movimentar a direção de um objeto de um game.
    /// </summary>
    public class JoystickVetorial : UserControl
    {
    

        private Control ctrlContainer;    //  control container para o desenho do joystick.

        private TimeReaction timeToUpdate; // tempo de atualização da situação do joystick.


        private MyImage AreaPadDirect2D; // imagem Direct2D da área ativa do joystick.

        public vetor2 PonteiroDirecao; // guarda a direção atual de movimento do joystick.
                                       // guarda a direção exportável do joystick. 
                                       // A idéia é que ao movimentar o ponteiro atual do joystick, 
                                       // isso é capturado como uma direção que pode servir de movimento
                                       // para algum objeto do game.



        private vetor2 ponteiroAtual;

        /// <summary>
        /// retorna o ângulo em graus do vetor de direção.
        /// </summary>
        public double AnguloDirecao {
            get
            {
                return PonteiroDirecao.GetAngle();
            } // get
        } // AnguloDirecao


        private vetor2 ponteiroDestino;// guarda de direção destino do joystick.
        private vetor2 ponteiroCentral;
        private vetor2 ponteiroAnterior;
        private double radious; // determina as dimensões da área ativa do joystick.

        private bool isSentidoHorarioDeRotacao = false;
        public JoystickVetorial(Point location, double radious, Control container)
        {

            this.ponteiroCentral = new vetor2(radious / 2.0, radious / 2.0);
            this.radious = radious; // dimensão da imagem da área ativa do joystick.

            this.Size = new Size((int)radious, (int)radious);
            this.Location = new Point(location.X, location.Y);
            this.BackColor = container.BackColor;
            this.timeToUpdate = new TimeReaction(1000.0 / 48.0);
            this.ctrlContainer = container;


            this.MouseMove += JoystickVetorial_MouseMove;
            this.MouseDown += JoystickVetorial_MouseDown;


            this.ponteiroAtual = new vetor2(radious, 0.0); // inicializa ó ponteiro de consulta de saída.
            this.ponteiroAtual = new vetor2(radious, 0.0); // inicializa o ponteiro de posição atual.
            this.ponteiroDestino = null;

        } // JoystickVetorial()

        private void JoystickVetorial_MouseDown(object sender, MouseEventArgs e)
        {
            this.ponteiroDestino = new vetor2(e.Location.X, e.Location.Y);
            this.isSentidoHorarioDeRotacao = this.IsSentidoHorario(this.ponteiroAtual, this.ponteiroDestino);
            this.Draw();
        }

        private void JoystickVetorial_MouseMove(object sender, MouseEventArgs e)
        {
            ponteiroDestino = new vetor2(e.Location.X, e.Location.Y);
            this.isSentidoHorarioDeRotacao = this.IsSentidoHorario(this.ponteiroAtual, this.ponteiroDestino);
            this.Draw();

        }

        public new void Update()
        {
            if ((this.timeToUpdate.IsTimeToAct()) && (this.ponteiroDestino != null))
            {
                double deltaAngulo = angulos.toRadianos(AnguloEntreVetores(this.ponteiroAtual - this.ponteiroCentral, this.ponteiroDestino - this.ponteiroCentral));
                //se os ponteiros atual e destinho não estiverem pertos, 
                //continua movimentando o ponteiro atual até chegar perto 
                //vetorialmente do vetor destino.
                if ((ponteiroAnterior != null) && (!isSentidoHorarioDeRotacao)) 
                {
                    ponteiroAnterior = new vetor2(ponteiroAtual);
                    ponteiroAtual = ponteiroAtual.RotacionaVetor(angulos.toRadianos(+deltaAngulo / 0.5));
                } // if
                else
                {
                    ponteiroAnterior = new vetor2(ponteiroAtual);
                    ponteiroAtual = ponteiroAtual.RotacionaVetor(angulos.toRadianos(-deltaAngulo / 0.5));
                } // else

                if (vetor2.Distancia(ponteiroAtual, ponteiroDestino) < 1.5)
                {
                    this.PonteiroDirecao = new vetor2(this.ponteiroAtual - this.ponteiroCentral); // guarda a posição e direção na variável de consulta fora do componente.
                  
                }//if
            }// if
        } // Update()

        public void Draw()
        {

            Point pontoCentral = new Point((int)(radious / 2.0), ((int)(radious / 2.0)));
            Point pontoDestino = new Point(0, 0);
            Point pontoAtual = new Point((int)this.ponteiroAtual.X, (int)this.ponteiroAtual.Y);

            if (ponteiroDestino != null)
                pontoDestino = new Point((int)this.ponteiroDestino.X, (int)this.ponteiroDestino.Y);



            Bitmap AreaInc = new Bitmap((int)radious, (int)radious);
            Graphics g = Graphics.FromImage(AreaInc);

            g.FillEllipse(new SolidBrush(Color.Blue), 0, 0, AreaInc.Width, AreaInc.Height); // desenha a circunferência da área ativa do joystick vetorial.

            if (ponteiroDestino != null)
                g.DrawLine(new Pen(Color.Yellow, 6.0f), pontoCentral, pontoDestino); // desenha o ponteiro de destino do vetor de direção do joystick.

            g.DrawLine(new Pen(Color.Red, 4.0F), pontoCentral, pontoAtual);// desenha o ponteiro atual do vetor de direção do joystick.

            this.AreaPadDirect2D = new MyImage(AreaInc, this, AreaInc.Size);
            this.AreaPadDirect2D.Begin();//inicia o desenho dos vetores direção.
            this.AreaPadDirect2D.Clear(this.BackColor);
            this.AreaPadDirect2D.Draw(new vetor2(0, 0));
            this.AreaPadDirect2D.End(); //finaliza o desenho dos vetores direção.

            // libera os recursos utilizados no desenho do componente.
            this.AreaPadDirect2D.Dispose();
            AreaInc.Dispose();
            g.Dispose();
        } // Draw()

        // calcula o ângulo em graus entre dois vetores.
        private double AnguloEntreVetores(vetor2 v1, vetor2 v2)
        {
            double n1 = v1.Modulo();
            double n2 = v2.Modulo();
            if ((n1 == 0.0) || (n2 == 0))
                return 100.0;
            double produtoEscalar = vetor2.ProdutoEscalar(v1, v2);
            return angulos.toGraus(Math.Acos(produtoEscalar / (n1 * n2)));
        } //  AnguloEntreVetores()

        //sentido anti-horário se omega>0
        //sentido horário se omega<0

        private bool IsSentidoHorario(vetor2 v1, vetor2 vdestino)
        {
            vetor2 vSentidoAntiHorario = new vetor2(v1);
            vetor2 vSentidoHorario = new vetor2(v1);

            vSentidoAntiHorario = vSentidoAntiHorario.RotacionaVetor(+1.0); // sentido anti-horário;

            vSentidoHorario = vSentidoHorario.RotacionaVetor(-1.0);// sentido horário,

            if (vetor2.Distancia(vSentidoAntiHorario, vdestino) < vetor2.Distancia(vSentidoHorario, vdestino))
                return false;
            if (vetor2.Distancia(vSentidoAntiHorario, vdestino) < vetor2.Distancia(vSentidoHorario, vdestino))
                return true;

            return true;

        } // IsClockWiseRotation()

        private double GetAngle(vetor2 ponteiroDirecao)
        {
            double anguloQuadrante = Math.Atan2(Math.Abs(ponteiroDirecao.Y), Math.Abs(ponteiroDirecao.X));
            if ((ponteiroDirecao.X < 0) && (ponteiroDirecao.Y > 0))
                anguloQuadrante += Math.PI / 2.0;
            if ((ponteiroDirecao.X < 0) && (ponteiroDirecao.Y < 0))
                anguloQuadrante += Math.PI;
            if ((ponteiroDirecao.X > 0) && (ponteiroDirecao.Y < 0))
                anguloQuadrante += (3.0 / 2.0) * Math.PI;

            return angulos.toGraus(anguloQuadrante);
        } // AnguloDirecao()
    }// class
}// namespace
