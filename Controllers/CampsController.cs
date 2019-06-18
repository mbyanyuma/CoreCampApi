using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetAllCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                CampModel[] model = _mapper.Map<CampModel[]>(results);

                return Ok(model);

            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);

                return ReturnStatus500InternalServerError();
            }

        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null)
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDateTime, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDateTime, includeTalks);

                if (!results.Any())
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

        private ObjectResult ReturnStatus500InternalServerError()
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, "shockingly, our database failed");
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps",
                    new {moniker = model.Moniker});

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Cannot use the current moniker");
                }

                //create a new camp
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created("", _mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }

            return BadRequest();
        }
    }
}