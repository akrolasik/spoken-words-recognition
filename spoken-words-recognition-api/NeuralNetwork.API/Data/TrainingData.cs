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
        public Matrix<float> ExpectedOutput;

        private Matrix<float> _input;

        public Matrix<float> Input()
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