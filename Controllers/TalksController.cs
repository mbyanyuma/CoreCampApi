using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Win32.SafeHandles;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        private ObjectResult ReturnStatus500InternalServerError()
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks for this moniker");
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);
                if (talks == null)
                {
                    return NotFound();
                }
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    BadRequest("The Camp does not exist");
                }

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null)
                {
                    BadRequest("Speaker could not be found");
                }

                talk.Speaker = speaker;

                _repository.Add(talk);

                if (await _repository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new {moniker, id = talk.TalkId});

                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }

            return BadRequest();
        }

        [HttpPut("{int: id}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                _mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update the database fam, p.s man utd > liverpool any day");
                }
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

        [HttpDelete("{int: id}")]
        public async Task<ActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null)
                {
                    return NotFound("failed to find the talk to delete");
                }

                _repository.Delete(talk);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("failed to delete the talk");
                }

                
            }
            catch (Exception)
            {
                return ReturnStatus500InternalServerError();
            }
        }

    }
}
