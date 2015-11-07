using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawaiiBiosReader
{
    public class GridRowVoltage2
    {
        private int _dpm;
        private String _position0;
        private int _value0;
        private String _position1;
        private int _value1;
        private String _position2;
        private int _value2;
        private String _position3;
        private int _value3;

        private String _unit;
        private String _type;



        public GridRowVoltage2(String pos0, int val0, String pos1, int val1, String pos2, int val2, String pos3, int val3, String un, String type, int dpm)
        {
            _dpm = dpm;

            _position0 = pos0;
            _value0 = val0;


            _position1 = pos1;
            _value1 = val1;


            _position2 = pos2;
            _value2 = val2;


            _position3 = pos3;
            _value3 = val3;

            _unit = un;
            _type = type;
        }

        public int dpm
        {
            get { return _dpm; }
            set { _dpm = value; }
        }


        public String position0
        {
            get { return _position0; }
            set { _position0 = value; }
        }
        public int value0
        {
            get { return _value0; }
            set { _value0 = value; }
        }
        public String position1
        {
            get { return _position1; }
            set { _position1 = value; }
        }
        public int value1
        {
            get { return _value1; }
            set { _value1 = value; }
        }
        public String position2
        {
            get { return _position2; }
            set { _position2 = value; }
        }
        public int value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }
        public String position3
        {
            get { return _position3; }
            set { _position3 = value; }
        }

        public int value3
        {
            get { return _value3; }
            set { _value3 = value; }
        }
        public String unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        public String type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
}
