﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/client")]
	[ApiController]
	public class ClientController : Controller
	{
		private ClientProcessor _clientProcessor;
		public ClientController(ClientProcessor clientProcessor)
		{
			_clientProcessor = clientProcessor;
		}
		[HttpPost]
		public IActionResult InsertClient([FromBody] JObject jsonClient)
		{
			Client client = Client.CreateFromJson(jsonClient);
			string clientId = _clientProcessor.InsertClient(client);
			if (clientId != null)
				return Ok(clientId);
			return BadRequest();
		}

		[HttpPut]
		public IActionResult UpdateClient([FromBody] JObject jsonClient)
		{
			Client client = Client.CreateFromJson(jsonClient);
			if (string.IsNullOrEmpty(client.ID))
				return BadRequest("Client ID is required");

			if (_clientProcessor.UpdateClient(client))
				return Ok("Client Updated");
			return BadRequest();
		}

		[HttpGet("{clientId}")]
		public IActionResult GetClientById([FromRoute(Name = "clientId")][Required] string clientId)
		{
			Client client = _clientProcessor.GetClientById(clientId);
			if (client == null)
				return BadRequest("Could not find client");
			return Json(client);
		}

		[HttpDelete("{clientId}")]
		public IActionResult DeleteClient([FromRoute(Name = "clientId")][Required] string clientId)
		{
			if (_clientProcessor.DeleteClient(clientId))
				return Ok("Client Deleted");
			return BadRequest("Failed to delete client");
		}
	}
}
