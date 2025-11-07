using Microsoft.ML;
using Microsoft.ML.Data;

namespace LeagueBuildTool.Core.ML
{
    /// <summary>
    /// Handles build recommendations using ML.NET
    /// </summary>
    public class BuildRecommender
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly string _modelPath;

        public BuildRecommender(string modelPath = "BuildRecommender.zip")
        {
            _mlContext = new MLContext(seed: 0);
            _modelPath = modelPath;
        }

        /// <summary>
        /// Trains the model using collected match data
        /// </summary>
        public async Task TrainModelAsync(List<BuildTrainingExample> trainingData)
        {
            // Convert training data to IDataView
            var data = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Create training pipeline
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ChampionEncoded", "ChampionName")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("RoleEncoded", "Role"))
                .Append(_mlContext.Transforms.Concatenate("Features", 
                    "ChampionEncoded", "RoleEncoded", "CurrentGold", "GameTime"))
                .Append(_mlContext.Transforms.NormalizeMinMax("NormalizedFeatures", "Features"))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: "WasWin",
                    featureColumnName: "NormalizedFeatures"));

            // Train the model
            _model = pipeline.Fit(data);

            // Save the model
            _mlContext.Model.Save(_model, data.Schema, _modelPath);
        }

        /// <summary>
        /// Predicts whether a build would be successful based on the current game state
        /// </summary>
        public float PredictBuildSuccess(BuildPredictionFeatures features)
        {
            if (_model == null)
            {
                if (File.Exists(_modelPath))
                {
                    LoadModel();
                }
                else
                {
                    throw new InvalidOperationException("Model not trained yet");
                }
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<BuildPredictionFeatures, BuildPrediction>(_model);
            var prediction = predictionEngine.Predict(features);

            return prediction.Probability;
        }

        /// <summary>
        /// Loads a previously trained model
        /// </summary>
        public void LoadModel()
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
            }
        }
    }

    /// <summary>
    /// Represents the output prediction from the ML model
    /// </summary>
    public class BuildPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedSuccess { get; set; }

        [ColumnName("Score")]
        public float Probability { get; set; }
    }
}