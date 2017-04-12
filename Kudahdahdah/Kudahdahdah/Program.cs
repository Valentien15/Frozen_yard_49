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

        }
    }

    class create
    {
        public static int flagOfCreations = 0;

        public static int ways = Console.Read();

        public static int lanes = Console.Read();

        public static int[] lanesArray = new int[lanes];

        public static int[] carsQty = new int[lanes];

        public static int[] lenghtLanes = new int[lanes];

        public static int[] wayArray = new int[ways];
        //больше нуля исток, меньше сток

        public static int[] turnOnLanes = new int[lanes];

        public static int[] timeOfWaiting = new int[lanes];

        public static int[] waysArrayBlocked = new int[lanes];

        public const int maxWaitingTime = 90, percentOfDelay = 9, maxQtyOfCarsOnOut = 50;

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

    class getCams
    {
        void getByComPort()
        {
            Random rndCars = new Random();
            for (int i = 0; i < create.lanes; i++)
            {
                int a = rndCars.Next(0, 1);
                create.carsQty[i] += a;
            }
        }
    }

    class brains
    {
        int[] priorityOfLanes = new int[create.lanes];
        int[] priorityOfLanesCopy = new int[create.lanes];
        int[] writedLanesToOn = new int[create.lanes];
        static int deltaTime = 0;
        Stopwatch stopWatch = new Stopwatch();

        void priority()
        {
            for (int i = 0; i < create.lanes; i++)
            {
                priorityOfLanes[i] = create.carsQty[i] / create.lenghtLanes[i];
            }
        }
        void createCopy()
        {
            for (int i = 0; i < create.lanes; i++)
            {
                priorityOfLanesCopy[i] = priorityOfLanes[i];
            }
        }
        void sortingPriority()
        {
            int minQty = 9999, minQtyPre = 0, iBuff = 0;
            for (int j = 0; j < create.lanes; j++)
            {
                for (int i = 0; i < create.lanes; i++)
                {
                    if (priorityOfLanesCopy[i] < minQty && priorityOfLanesCopy[i] > minQtyPre)
                    {
                        minQty = priorityOfLanesCopy[i];
                        iBuff = i;
                    }
                }
                priorityOfLanes[create.lanes - j] = iBuff;
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
                for (int i = 0; i < create.lanes; i++)
                {
                    create.timeOfWaiting[i]++;
                    if (create.timeOfWaiting[i] > create.maxWaitingTime - create.percentOfDelay * create.maxWaitingTime / 100)
                        for (int j = 1; j < create.lanes; j++)
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
            for (int i = 0; i < create.lanes; i++)
            {
                if (getWayByLane(i) == incomeWay)
                {
                    qtyCarsInFunc += create.carsQty[i];
                }
            }
            if (qtyCarsInFunc > (-1) * create.maxQtyOfCarsOnOut * create.wayArray[incomeWay])
                return 1;
            else
                return 0;
        }

        int getDataNoExit()
        {
            int a = 0;
            for (int i = 0; i < create.lanes; i++)
            {
                if (create.wayArray[i] < 0 && fullOutput(create.wayArray[i]) == 1)
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
            for (int i = 0; i < create.lanes; i++)
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
                if (create.waysArrayBlocked[a1 % 10] == 1 || create.waysArrayBlocked[a2 % 10] == 1)
                {
                    create.turnOnLanes[i] = 0;
                }
            }
        }

        public int getWayByLane(int lane)
        {
            int way = 0, laneBuff = lane;
            for (int i = 0; i < create.lanes; i++)
            {
                laneBuff -= create.wayArray[i];
                if (laneBuff < create.wayArray[i + 1])
                    way = i;
            }
            return way;
        }
        int findAbsoluteWay(int lane)
        {
            int way = getWayByLane(lane);
            int cross = create.lanesArray[way + lane];
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
        int cross(int a, int lane)
        {
            if (a == 1)//запись в массив включенных
            {
                writedLanesToOn[lane] = 1;
                return 1488;
            }
            if (a == 0)//проверка качества пересечения
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
                for (int i = 0; i < create.lanes; i++)
                {
                    if (writedLanesToOn[i] == 1)
                    {
                        if (checkCrossing(absolWay1, i) == 1 && checkCrossing(absolWay2, i) == 1)
                            crossing++;
                        else
                        {; }
                    }
                }
                if (crossing == 0)
                    return 0;
                else
                    return 1;
            }
            else
                return 1488;
        }

        void turnOnWay()
        {
            for (int i = 0; i < create.lanes; i++)
            {
                if (cross(0, priorityOfLanes[i]) == 0)
                {
                    cross(1, priorityOfLanes[i]);
                    create.turnOnLanes[i] = 1;
                }
            }
        }
        public
        void turnOnLaneGroup()
        {
            int qtyOfOnLanes = 0, maxQtyOfLanes = 0;

            for (int i = 0; i < create.lanes; i++)
            {
                if (cross(0, priorityOfLanes[i]) == 0)
                {
                    cross(1, priorityOfLanes[i]);
                    create.turnOnLanes[i] = 1;
                }
                qtyOfOnLanes++;

            }
        }

        class simulaton
        {
            public static int[] distanceCarForward = new int[create.lanes];
            public static int[] timeOfWork = new int[create.lanes];
            public static int qty = 0;
            public static int l1 = 0, l2 = 1; //l1-прав. нижн. угол, l2-лев.нижн. угол
            void renewTimeOfWork()
            {
                for (int i = 0; i < create.lanes; i++)
                {
                    if (create.turnOnLanes[i] == 1 && create.turnOnLanes[i] > 0)
                    {
                        timeOfWork[i]++;
                    }
                    if (create.turnOnLanes[i] == 1 && create.turnOnLanes[i] < 0)
                    {
                        timeOfWork[i] = 1;
                    }
                }
            }

            void centeralFunc()
            {
                int qtyBuff = 0;
                for (int i = 0; i < create.lanes; i++)
                {
                    if (create.turnOnLanes[i] == 1)
                    {
                        qtyBuff = leavedCars(timeOfWork[i]) - leavedCars(timeOfWork[i] - 1);
                        create.carsQty[i] -= qtyBuff;
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
                    for (int j = 0; j < create.lanes; j++)
                    {
                        if (k == rnd.Next(1, 100) % (season + 1))
                            create.carsQty[j]++;
                    }
                }
            }

            void outMetaData()//выводит в консоль
            {
                qty = 0;
                for (int i = 0; i < create.lanes; i++)
                {
                    Console.Write(create.carsQty[i]); //количество машин
                    Console.Write(timeOfWork[i]); //время работы полос
                    Console.Write(create.turnOnLanes[i]); //состояние полос
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
                for (int i = 0; i < create.lanes; i++)
                {
                    if (create.turnOnLanes[i] == 1)
                    {
                        //printMovingCars(i);
                    }
                }
            }
        }
    }
}
