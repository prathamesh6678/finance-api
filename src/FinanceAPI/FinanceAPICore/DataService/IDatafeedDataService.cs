﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface IDatafeedDataService
	{
		public List<Datafeed> GetDatafeeds(string clientId);
		public bool AddUpdateClientDatafeed(Datafeed datafeed);
		public bool UpdateAccessKey(string newAccessKey, string newRefreshToken, string oldAccessKey, DateTime lastUpdated);
		public string GetRefreshTokenByAccessKey(string encryptedAccesskey);
		public bool AddAccountDatafeedMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID);
		public bool RemoveAccountDatafeedMapping(string clientId, string accountID);
		public bool IsExternalAccountMapped(string clientId, string externalAccountID, string vendorID, out string mappedAccount);
		public string GetExternalAccountMapping(string accountId);
	}
}
