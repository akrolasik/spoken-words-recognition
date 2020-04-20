using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Repository;
using NeuralNetwork.API.Statistics;

namespace NeuralNetwork.API.Controllers
{
    [ApiController]
    [Route("evolution")]
    public class EvolutionController : ControllerBase
    {
        private readonly IEvolutionRepository _evolutionRepository;

        public EvolutionController(IEvolutionRepository evolutionRepository)
        {
            _evolutionRepository = evolutionRepository;
        }

        [HttpPut]
        public IActionResult AddEvolution([FromBody] EvolutionConfig evolutionConfig)
        {
            _evolutionRepository.AddEvolution(evolutionConfig);
            return Ok();
        }

        [HttpGet]
        public ActionResult<List<EvolutionConfig>> GetEvolutions()
        {
            return Ok(_evolutionRepository.GetEvolutions());
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public IActionResult DeleteEvolution([FromRoute] Guid id)
        {
            _evolutionRepository.DeleteEvolution(id);
            return Ok();
        }

        [HttpPost]
        [Route("start/{id}")]
        public IActionResult StartEvolution([FromRoute] Guid id)
        {
            _evolutionRepository.StartEvolution(id);
            return Ok();
        }

        [HttpGet]
        [Route("statistics/{id}")]
        public ActionResult<NeuralNetworkStatistics> GetEvolutionStatistics([FromRoute] Guid id)
        {
            return Ok(_evolutionRepository.GetNeuralNetworkStatistics(id));
        }

        [HttpDelete]
        [Route("stop")]
        public IActionResult StopEvolution()
        {
            _evolutionRepository.StopRunningEvolution();
            return Ok();
        }
    }
}
