using System;
namespace Utils
{

    /// <summary>
    /// implementa o gerenciamento do tempo no jogo.
    /// </summary>
    public class TimeGame
    {

        private DateTime dtLastCicle = DateTime.Now;
        private double dtMillisecoundsPast = 0.0;
        /// <summary>
        /// obtém o tempo absoluto em milisegundos.
        /// </summary>
        /// <returns></returns>
        public double GetTime()
        {
            DateTime dt = DateTime.Now;
            double timePassed = (dt - dtLastCicle).TotalMilliseconds;
            dtLastCicle = DateTime.Now;
            return timePassed;
        } //GetTime()

        /// <summary>
        /// obtém o tempo passado entre o último ciclo do jogo e o ciclo atual.
        /// </summary>
        /// <returns></returns>
        public double GetElapsedTime()
        {
            double TimeNow = GetTime();
            double timePassed = TimeNow - dtMillisecoundsPast;
            dtMillisecoundsPast = GetTime();
            return timePassed;
        } // GetElapsedTime()
    }  // class TimeGame


    /// <summary>
    /// classe que encapsula o tempo de reação para determinada atividade. É semelhante ao [TimeGame.GetElapsedTime()],
    /// mas contém algumas funcionalidade a mais, além de esconder as contas feitas do tempo percorrido.
    /// </summary>
    public class TimeReaction
    {
        private double TimeBeginCycle;
        private double TimeFire;
        private TimeGame time = new TimeGame();
        private double timeAcumulated = 0.0;
        
        /// <summary>
        /// seta um temporizador.
        /// </summary>
        /// <param name="timeInMillsec">frames por segundo para um ciclo.</param>
        public TimeReaction(double timeInMillsec)
        {
            /// x fps---> 1000 mlsec;
            /// 1 fps----> y mlsec.---> y=1000.0/fps
            /// 
            this.TimeFire = timeInMillsec;
            this.TimeBeginCycle = time.GetTime();
        } // TimeRection()

        /// <summary>
        /// retorna [true] se passou o tempo de reação, [false] se não passou o tempo de reação.
        /// conta o tempo restante para o próximo ciclo.
        /// </summary>
        /// <returns>retorna [true] para ocorreu o tempo de reagir, [false] se não.</returns>
        public bool IsTimeToAct()
        {
            this.timeAcumulated+= time.GetTime();
            if ((this.timeAcumulated+this.TimeBeginCycle)>this.TimeFire)
            {
                this.timeAcumulated = (this.timeAcumulated + this.TimeBeginCycle) - this.TimeFire;
                TimeBeginCycle = time.GetTime();
                return true;
            } // if
            return false;
        } //IsTimeToAct()

      
        /// <summary>
        /// seta o tempo de disparo do componente.
        /// </summary>
        /// <param name="tempo"></param>
        public void SetTime(double tempo)
        {
            this.TimeFire = tempo;
            this.TimeBeginCycle = time.GetTime();
            this.timeAcumulated = 0.0;
        } // SetTime()
        /// <summary>
        /// otém o tempo de disparo do componetne.
        /// </summary>
        /// <returns></returns>
        public double GetTime()
        {
            return this.TimeFire;
        }

    }// class TimeReaction

    /// <summary>
    /// classe para contagem de tempo total.
    /// </summary>
    public class TimeTotal
    {
        private DateTime tempo;
        private double tempoTotal = 0.0;
        private double tempoPassado = 0.0;

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="tempoTotal">tempo total a ser verificado.</param>
        public TimeTotal(double tempoTotal)
        {
            this.tempo = new DateTime();
            this.tempoTotal = tempoTotal;
        } // TimeTotal()
        /// <summary>
        /// verifica se o tempo passado ultrapassou o tempo total.
        /// </summary>
        /// <returns></returns>
        public bool IsTimeComplet()
        {
            double tempoPercorrido = tempo.Millisecond - tempoPassado;
            this.tempoPassado += tempoPercorrido;

            return (tempoPassado > tempoTotal);
        } // GetTimeTotal()
            
    } // class TimeTotal
   
} // namespace
