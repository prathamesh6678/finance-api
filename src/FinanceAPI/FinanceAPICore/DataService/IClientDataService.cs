﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface IClientDataService
	{
		bool InsertClient(Client client);
		Client GetClientById(string clientId);
		bool UpdateClient(Client client);
		bool DeleteClient(string clientId);
	}
}
