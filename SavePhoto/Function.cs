using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.Rekognition.Model;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SavePhoto
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        private string _eTag;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }
        
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(S3Event evnt, ILambdaContext context)
        {            
            context.Logger.LogLine("Received: " + context.InvokedFunctionArn);

            _eTag = "eda53d85dcc5f83f2b7ef03af70fe88d";

            try
            {
                var recommendation = GenerateRecommendation();
                var response = GenerateResponse(recommendation);
                return response;
            }
            catch(Exception e)
            {
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }

        private SearchFacesByImageResponse SearchPhotosByImage()
        {
            //var s3ObjectResponse = S3Client.GetObjectAsync("recongimages", _eTag);
            var getObjectRequest = new GetObjectRequest();
            getObjectRequest.EtagToMatch = _eTag;
            getObjectRequest.BucketName = "recongimages";
            var s3ObjectResponse = S3Client.GetObjectAsync(getObjectRequest);
            //var objectName = s3ObjectResponse.Result.Key;
            var rekognitionClient = new Amazon.Rekognition.AmazonRekognitionClient("AKIAJASZFMUMX6B4JUIQ", "4YDS6cYGyTkES76EwwbLU/0KL1O7lO8YQGpsi2zV", Amazon.RegionEndpoint.APSoutheast2);
            var requestSearch = new SearchFacesByImageRequest();
            requestSearch.MaxFaces = 1;
            //requestSearch.CollectionId
            var s3Object = new Amazon.Rekognition.Model.S3Object();
            s3Object.Name = s3ObjectResponse.Result.Key;
            var image = new Image();
            image.S3Object = s3Object;
            requestSearch.Image = image;
            //requestSearch.FaceMatchThreshold
            var response = rekognitionClient.SearchFacesByImageAsync(requestSearch);
            return response.Result;
        }

        private string GetPurchaseHistory()
        {
            return "1. Corona\n2. ";
        }

        private string GenerateRecommendation()
        {
            //var searchResponse = SearchPhotosByImage();
            switch (new Random().Next(0, 3))
            {
                case 0:
                    return "Corona";
                case 1:
                    return "Carlton Dry";
                case 2:
                    return "Carlton Draught";
            }
            return "Victoria Bitter";
        }

        private string GenerateResponse(string recommendation)
        {
            return "{    “etag” : “" + _eTag + "”,    “recommendation” : “" + recommendation + "”}";
        }
    }
}
