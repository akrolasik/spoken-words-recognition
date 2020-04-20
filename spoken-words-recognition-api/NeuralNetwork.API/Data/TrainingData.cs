using MathNet.Numerics.LinearAlgebra;
using Utils.Model;

namespace NeuralNetwork.API.Data
{
    public class TrainingData
    {
        private readonly DataProvider _dataProvider;

        public TrainingData(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public int OutputIndex;
        public Recording Recording;
        public Matrix<double> ExpectedOutput;

        private Matrix<double> _input;

        public Matrix<double> Input()
        {

            if (_input == null)
            {
                lock (_dataProvider)
                {
                    _input = _dataProvider.GetInput(Recording).Result;
                }
            }

            return _input;
        }
    }
}