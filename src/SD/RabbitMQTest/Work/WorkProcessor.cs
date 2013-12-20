using System;
using System.Threading;

namespace NMSD.Cronus.Core.Multithreading.Work
{
    /// <summary>
    /// This class represents a wraper of a thread which does countinous work over a work source
    /// </summary>
    internal class WorkProcessor
    {

        string name;

        volatile bool shouldStop = false;

        private Thread thread;

        /// <summary>
        /// Creates an instace of a crawler.
        /// </summary>
        /// <param name="name">Defines the thread name of the crawler instance</param>
        public WorkProcessor(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Starts a dedicated to the crawler's thread.Which countinously takes and executes work.
        /// </summary>
        /// <param name="workSource">Crawler worksource</param>
        public void StartCrawling(WorkSource workSource)
        {
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(() =>
                {
                    while (!shouldStop)
                    {
                        IWork work;
                        try
                        {

                            work = workSource.GetAvailableWork();
                            if (work != null)
                            {
                                work.Start();

                                workSource.ReturnFinishedWork(work);

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    
                }));
                thread.Name = name;
                thread.Start();
            }
            else
            {

                throw new InvalidOperationException(String.Format("Crawler '{0}' is already running on another source.", name));
            }
        }

        /// <summary>
        /// Tells to the crawler's thread to finish it's last work and exit.
        /// </summary>
        public void Stop()
        {
            
            thread = null;
            shouldStop = true;
        }

    }
}