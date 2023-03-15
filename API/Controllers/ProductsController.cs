using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;
using Core.Specifications;
using API.Dtos;
using AutoMapper;
using API.Errors;
using Microsoft.AspNetCore.Http;
using API.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<ProductBrand> _productBrandRepo;
        private readonly IGenericRepository<ProductType> _productTypeRepo;
        private readonly IGenericRepository<Product> _productsRepo;
         private readonly IGenericRepository<ProductReview> _productsReviewRepo;
          private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public ProductsController(IGenericRepository<Product> productsRepo,
            IGenericRepository<ProductType> productTypeRepo,
            IGenericRepository<ProductBrand> productBrandRepo,
             IGenericRepository<ProductReview> productsReviewRepo,
             IOrderService orderService,
            IMapper mapper)
        {
            _mapper = mapper;
            _productsRepo = productsRepo;
            _productTypeRepo = productTypeRepo;
            _productBrandRepo = productBrandRepo;
            _productsReviewRepo=productsReviewRepo;
             _orderService = orderService;
        }

        //[Cached(600)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery] ProductSpecParams productParams)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
            var countSpec = new ProductsWithFiltersForCountSpecification(productParams);

            var totalItems = await _productsRepo.CountAsync(countSpec);

            var products = await _productsRepo.ListAsync(spec);

            var data = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

            return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex,
                productParams.PageSize, totalItems, data));
        }

        //[Cached(600)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);

            var product = await _productsRepo.GetEntityWithSpec(spec);

            if (product == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<ProductToReturnDto>(product);
        }

        //[Cached(600)]
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            return Ok(await _productBrandRepo.ListAllAsync());
        }

        //[Cached(600)]
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetTypes()
        {
            return Ok(await _productTypeRepo.ListAllAsync());
        }

        [HttpGet("reviews/{id}")]
        public async Task<ActionResult<IReadOnlyList<ProductReview>>> GetReviews(int id)
        {
            return Ok(await _productsReviewRepo.ListAllAsync(id));
        }

         [HttpPost("reviews/{id}")]
        public async Task<ActionResult<ProductReviewDto>> PostReviews(int id, ProductReviewDto productReviewDto)
        {
            var review = new ProductReview
            {
                UserName = productReviewDto.Name,
                Summary = productReviewDto.Comment,
                Review = "",
                ProductId = id
            };

            var predictionResponse = await GetPrediction(review.Summary);
            //review.Review = predictionResponse.Prediction;
            var rating =int.Parse(predictionResponse.Prediction);
            if (rating >= 3)
            {
                review.Review = "Positive";
            }
            else
            {
                review.Review = "Negative";
            }

            var response = await _orderService.CreateReview(review);

            if (response == false) return BadRequest(new ApiResponse(400, "Problem posting review"));
            //review.Review = "Positive";//api call here from the model //input is just a summary i.e. comment

            return Ok(review);
        }

        public class PredictionResponse
        {
            public string Prediction { get; set; }
            public string Text { get; set; }
        }

        // Define a function to make the API request
        public async Task<PredictionResponse> GetPrediction(string reviewText)
        {
            // Define the request body
            var requestBody = new StringContent(JsonConvert.SerializeObject(new { text = reviewText }));
            requestBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Create the HTTP client and send the request
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("http://127.0.0.1:6060/predict", requestBody);

            // Read the response content and deserialize it into a PredictionResponse object
            var responseContent = await response.Content.ReadAsStringAsync();
            var predictionResponse = JsonConvert.DeserializeObject<PredictionResponse>(responseContent);

            return predictionResponse;
        }
    }
}

        