﻿using System;

namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon
{
    public class EventTester : IEventabe
    {
        public int LisCount;
        event Action _IEventabe2Event1;
        event Action IEventabe2.Event21
        {
            add
            {
                
                _IEventabe2Event1 += value;
                LisCount++;
            }

            remove
            {
                
                _IEventabe2Event1 -= value;
                LisCount--;
            }
        }

        event Action _IEventabe1Event1;
        event Action IEventabe1.Event1
        {
            add
            {
                
                _IEventabe1Event1 += value;
                LisCount++;
            }

            remove
            {
                
                _IEventabe1Event1 -= value;
                LisCount--;
            }
        }

        event Action<int> _IEventabe2Event2;
        event Action<int> IEventabe2.Event22
        {
            add
            {
                
                _IEventabe2Event2 += value;
                LisCount++;
            }

            remove
            {
                
                _IEventabe2Event2 -= value;
                LisCount--;
            }
        }

        event Action<int> _IEventabe1Event2;
        event Action<int> IEventabe1.Event2
        {
            add
            {
                
                _IEventabe1Event2 += value;
                LisCount++;
            }

            remove
            {
                
                _IEventabe1Event2 -= value;
                LisCount--;
            }
        }

        public void Invoke11()
        {
            _IEventabe1Event1();
        }

        public void Invoke21()
        {
            _IEventabe2Event1();
        }

        public void Invoke12(int val)
        {
            _IEventabe1Event2(val);
        }

        public void Invoke22(int val)
        {
            _IEventabe2Event2(val);
        }
    }
}
