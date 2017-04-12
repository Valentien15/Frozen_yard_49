using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;

namespace ProgramForIntersection
{

    class Program
    {
        static void Main(string[] args)
        {
            ACreate create = new ACreate();
            create.initFromConsole();

            AGetCams getCams = new AGetCams();

            ABrains brains = new ABrains(create);

            ASimulaton simulation = new ASimulaton(create);

            while (true)
            {
                getCams.getByComPort(create);
                
            }
            

        }
    }

    class ACreate
    {
        public readonly int maxWaitingTime = 90;

        public readonly int percentOfDelay = 9;

        public readonly int maxQtyOfCarsOnOut = 50;

        public int flagOfCreations = 0;

        public int ways = 0;

        public int lanes = 0;

        public int[] lanesArray = null;

        public int[] carsQty = null;

        public int[] lenghtLanes = null;

        public int[] wayArray = null;
        //больше нуля исток, меньше сток

        public int[] turnOnLanes = null;

        public int[] timeOfWaiting = null;

        public int[] waysArrayBlocked = null;


        public void initFromConsole()
        {
            ways = Console.Read();
            lanes = Console.Read();
            lanesArray = new int[lanes];
            carsQty = new int[lanes];
            lenghtLanes = new int[lanes];
            wayArray = new int[ways];
            turnOnLanes = new int[lanes];
            timeOfWaiting = new int[lanes];
            waysArrayBlocked = new int[lanes];


            getRoadMark();
            fullNeedArrays();
        }


        private void fullNeedArrays()
        {
            for (int i = 0; i < lanes; i++)
            {
                carsQty[i] = 0;
                turnOnLanes[i] = 0;
                timeOfWaiting[i] = 0;
            }
        }
        private void getRoadMark()
        {
            for (int i = 0; i < lanes; i++)
            {
                lanesArray[i] = Console.Read();
            }
        }
    }

    class AGetCams
    {
        public void getByComPort(ACreate create)
        {
            if (create == null)
            {
                return;
            }

            Random rndCars = new Random();
            for (int i = 0; i < create.lanes; i++)
            {
                int a = rndCars.Next(0, 1);
                create.carsQty[i] += a;
            }
        }
    }

    class ABrains
    {
        private readonly ACreate mCreate = null;



        private int[] priorityOfLanes = null;
        private int[] priorityOfLanesCopy = null;
        private int[] writedLanesToOn = null;
        private int deltaTime = 0;
        Stopwatch stopWatch = new Stopwatch();

        public ABrains(ACreate create)
        {
            mCreate = create;
            priorityOfLanes = new int[mCreate.lanes];
            priorityOfLanesCopy = new int[mCreate.lanes];
            writedLanesToOn = new int[mCreate.lanes];
        }



        void priority()
        {
            for (int i = 0; i < mCreate.lanes; i++)
            {
                priorityOfLanes[i] = mCreate.carsQty[i] / mCreate.lenghtLanes[i];
            }
        }
        void createCopy()
        {
            for (int i = 0; i < mCreate.lanes; i++)
            {
                priorityOfLanesCopy[i] = priorityOfLanes[i];
            }
        }
        void sortingPriority()
        {
            int minQty = 9999, minQtyPre = 0, iBuff = 0;
            for (int j = 0; j < mCreate.lanes; j++)
            {
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    if (priorityOfLanesCopy[i] < minQty && priorityOfLanesCopy[i] > minQtyPre)
                    {
                        minQty = priorityOfLanesCopy[i];
                        iBuff = i;
                    }
                }
                priorityOfLanes[mCreate.lanes - j] = iBuff;
                minQtyPre = minQty;
            }
        }

        int StopWatch()
        {
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            return ts.Milliseconds;
        }

        void changePriorityByTime()
        //если полоса ждет больше какого то времени
        //то она включается на автомате
        {
            deltaTime += StopWatch();
            if (deltaTime > 1000)
            {
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    mCreate.timeOfWaiting[i]++;
                    if (mCreate.timeOfWaiting[i] > mCreate.maxWaitingTime - mCreate.percentOfDelay * mCreate.maxWaitingTime / 100)
                        for (int j = 1; j < mCreate.lanes; j++)
                        {
                            if (priorityOfLanes[j] == i)
                            {
                                j++;
                            }
                            else
                            {
                                priorityOfLanes[j] = priorityOfLanes[j - 1];
                            }
                        }
                    priorityOfLanes[0] = i;
                }
            }

        }

        void StartWatch()
        {
            stopWatch.Start();
        }

        int fullOutput(int incomeWay)
        {
            int qtyCarsInFunc = 0;
            for (int i = 0; i < mCreate.lanes; i++)
            {
                if (getWayByLane(i) == incomeWay)
                {
                    qtyCarsInFunc += mCreate.carsQty[i];
                }
            }
            if (qtyCarsInFunc > (-1) * mCreate.maxQtyOfCarsOnOut * mCreate.wayArray[incomeWay])
                return 1;
            else
                return 0;
        }

        int getDataNoExit()
        {
            int a = 0;
            for (int i = 0; i < mCreate.lanes; i++)
            {
                if (mCreate.wayArray[i] < 0 && fullOutput(mCreate.wayArray[i]) == 1)
                {
                    a = 1;
                }
            }
            if (a == 1)
                return 1;
            else
                return 0;
        }

        void changePriorityByNoExit()
        //отключает полосы у которых выходы закрыты
        //не связано с мозгами, ТУПО отключает
        {
            for (int i = 0; i < mCreate.lanes; i++)
            {
                int a = findAbsoluteWay(i), a1 = 0, a2 = 0;
                if (a > 99)
                {
                    a1 = findAbsoluteWay(i) / 100 + findAbsoluteWay(i) % 10;
                    a2 = findAbsoluteWay(i) / 100 + (findAbsoluteWay(i) / 10) % 10;
                }
                else
                {
                    a1 = findAbsoluteWay(i);
                    a2 = findAbsoluteWay(i);
                }
                if (mCreate.waysArrayBlocked[a1 % 10] == 1 || mCreate.waysArrayBlocked[a2 % 10] == 1)
                {
                    mCreate.turnOnLanes[i] = 0;
                }
            }
        }

        public int getWayByLane(int lane)
        {
            int way = 0, laneBuff = lane;
            for (int i = 0; i < mCreate.lanes; i++)
            {
                laneBuff -= mCreate.wayArray[i];
                if (laneBuff < mCreate.wayArray[i + 1])
                    way = i;
            }
            return way;
        }
        int findAbsoluteWay(int lane)
        {
            int way = getWayByLane(lane);
            int cross = mCreate.lanesArray[way + lane];
            int a;
            if (way + cross >= 7)
                a = way + cross - 8;
            else
                a = way + cross;
            int qtyAns;
            if (cross % 2 == 1)
            {
                qtyAns = way * 100 + (a - 1) * 10 + a;
            }
            else
            {
                qtyAns = way * 10 + (a);
            }
            return qtyAns;
        }
        int checkCrossing(int checkingLane, int turnedOnLane)
        {
            if ((checkingLane % 10 - turnedOnLane % 10) == 2 || (checkingLane % 10 - turnedOnLane % 10) == -2)
                return 1;
            else
                return 0;
        }


        void setCross(int lane)
        {
            //запись в массив включенных
            writedLanesToOn[lane] = 1;
        }


        bool isCross(int lane)
        {
            int absolWay = findAbsoluteWay(lane);
            int absolWay1 = 0, absolWay2 = 0;
            int crossing = 0;
            if (absolWay > 99)
            {
                absolWay1 = (absolWay / 100) * 10 + absolWay % 10;
                absolWay2 = (absolWay / 100) * 10 + ((absolWay % 100) / 10);
            }
            if (absolWay <= 99)
            {
                absolWay1 = absolWay;
                absolWay2 = absolWay;
            }
            for (int i = 0; i < mCreate.lanes; i++)
            {
                if (writedLanesToOn[i] == 1)
                {
                    if (checkCrossing(absolWay1, i) == 1 && checkCrossing(absolWay2, i) == 1)
                        crossing++;
                    else
                    {; }
                }
            }

            return crossing == 0 ? false : true;
        }

        void turnOnWay()
        {
            for (int i = 0; i < mCreate.lanes; i++)
            {
                if (cross(0, priorityOfLanes[i]) == 0)
                {
                    cross(1, priorityOfLanes[i]);
                    mCreate.turnOnLanes[i] = 1;
                }
            }
        }
        public
        void turnOnLaneGroup()
        {
            int qtyOfOnLanes = 0, maxQtyOfLanes = 0;

            for (int i = 0; i < mCreate.lanes; i++)
            {
                if (isCross(priorityOfLanes[i]))
                {
                    setCross(priorityOfLanes[i]);
                    mCreate.turnOnLanes[i] = 1;
                }
                qtyOfOnLanes++;

            }
        }

    }
        class ASimulaton
        {
            private readonly ACreate mCreate = null;

            public int[] distanceCarForward = null;
            public int[] timeOfWork = null;
            public int qty = 0;
            public int l1 = 0, l2 = 1; //l1-прав. нижн. угол, l2-лев.нижн. угол

            public ASimulaton(ACreate create)
            {
                mCreate = create;
                distanceCarForward = new int[mCreate.lanes];
                timeOfWork = new int[mCreate.lanes];
            }

            void renewTimeOfWork()
            {
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    if (mCreate.turnOnLanes[i] == 1 && mCreate.turnOnLanes[i] > 0)
                    {
                        timeOfWork[i]++;
                    }
                    if (mCreate.turnOnLanes[i] == 1 && mCreate.turnOnLanes[i] < 0)
                    {
                        timeOfWork[i] = 1;
                    }
                }
            }

            void centeralFunc()
            {
                int qtyBuff = 0;
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    if (mCreate.turnOnLanes[i] == 1)
                    {
                        qtyBuff = leavedCars(timeOfWork[i]) - leavedCars(timeOfWork[i] - 1);
                        mCreate.carsQty[i] -= qtyBuff;
                        qty += qtyBuff;
                    }
                }
            }

            int leavedCars(int t) //расчет машин, которые выехали из полосы
            {
                double qty, a = 6, len = 9;
                if (t < 7)
                {
                    qty = a * Math.Pow(t, 2) / (len * 2);
                }
                else
                {
                    qty = a * Math.Pow(7.001, 2) / (len * 2) + (t - 7.1) * 40 / len;
                }
                return Convert.ToInt32(qty);
            }

            void spawn(int season) //спавнит машины
            {
                Random rnd = new Random();
                double qty = 15 * Math.Pow((3 - season), 2);
                int k = 0;
                for (int i = 0; i < qty; i++)
                {
                    for (int j = 0; j < mCreate.lanes; j++)
                    {
                        if (k == rnd.Next(1, 100) % (season + 1))
                            mCreate.carsQty[j]++;
                    }
                }
            }

            void outMetaData()//выводит в консоль
            {
                qty = 0;
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    Console.Write(mCreate.carsQty[i]); //количество машин
                    Console.Write(timeOfWork[i]); //время работы полос
                    Console.Write(mCreate.turnOnLanes[i]); //состояние полос
                }
                Console.Write(qty); //общее количество выехавших машин
            }

            /*void printMovingCars(int incomeLane)
            {
                if (brains.getWayByLane(incomeLane) == 0 || brains.getWayByLane(incomeLane) == 1)
                {

                }
                if (brains.getWayByLane(incomeLane) == 0 || brains.getWayByLane(incomeLane) == 1)
                {

                }
                if (brains.getWayByLane(incomeLane) == 0 || brains.getWayByLane(incomeLane) == 1)
                {

                }
                if (brains.getWayByLane(incomeLane) == 0 || brains.getWayByLane(incomeLane) == 1)
                {

                }
            }*/

            void outGraphics()
            {
                for (int i = 0; i < mCreate.lanes; i++)
                {
                    if (mCreate.turnOnLanes[i] == 1)
                    {
                        //printMovingCars(i);
                    }
                }
            }
    }
}
