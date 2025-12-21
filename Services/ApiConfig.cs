namespace figjam2.Services
{
    public static class ApiConfig
    {
        // Base URLs
        public const string CUSTOMERS_API_BASE = "https://customers-api.azurewebsites.net/api";
        public const string API_IDC_BASE = "https://api-idc.azurewebsites.net/api";

        // Endpoints
        public const string TCKN_GSM = $"{CUSTOMERS_API_BASE}/customer/tckn-gsm?code=gww5m66SOHBjQ9LY58dM5Gulq2giauLokvvIX4ylR405AzFu7CUbIA==";
        public const string KVKK_TEXT = $"{API_IDC_BASE}/kvkk/text/{{id}}?code=5OaiAxOi6mmwXV4gPTcKiHc9plIMg3s6Kcer667-4OK1AzFulZ-Mkw==";
        public const string KVKK_ONAY = $"{API_IDC_BASE}/kvkk/onay?code=VYa4cMvucKPZrC9eQ9ZuXKixMPhzb3M4URamT57nMXtkAzFuUL-6og==";
        public const string GENERATE_OTP = $"{API_IDC_BASE}/generate-otp?code=NgHo3RJwMJ4rVsPtGLau40m_vykzGV24zBAZYJPbVQpIAzFurGPRTw==";
        public const string SEND_OTP_SMS = $"{API_IDC_BASE}/send-otp-sms?code=fsfjOxq_5dhmOKxp4Q6ffi5ZuxdmGO2cnwGi2IBwRzSjAzFu9FrxaQ==";
        public const string VERIFY_OTP = $"{API_IDC_BASE}/verify-otp?code=NUqFOooe6OqRE8Nf5Se8Swlt5vUxjdMr3oJZes_-MioAAzFuP2nw2g==";
        public const string DUMMY_REPORT_LIST = $"{API_IDC_BASE}/dummy/report-list?code=09wQ_IdxHgsj4oPYrxSrQMbedNddK56Q60lsZPpS5CckAzFu3c0m1A==";
        public const string REPORT_DETAIL = $"{API_IDC_BASE}/GetReportDetail?code=ghMl1aeJa-tKFvo9hkAn3Cnzgkbf_sc3ZYg7tWvPLfLhAzFuAm67hQ==";

        public static string GetReportDetailUrl(string reportId) => $"{REPORT_DETAIL}&reportId={reportId}";
        public const string CUSTOMER_ADDRESS = $"{CUSTOMERS_API_BASE}/customer/addressfull/{{customerId}}?code=rNlR8hjUVN2GeCrR0Ac7px6kKA-pSeI-swMngYss39FAAzFuULEzbQ==";
        public const string CUSTOMER_ADDRESS_CREATE = $"{CUSTOMERS_API_BASE}/customer/address?code=HiKhittOb6dIjhw7YqA-W1W51iKtQ6D1UoHmKQl91rYSAzFuOurQTQ==";
        public const string CUSTOMER_JOB_INFO = $"{CUSTOMERS_API_BASE}/customer/job-infonew/{{customerId}}?code=RPR5Pwi9E_TDXZb1f27lPNrk7jMF67Et58elqdNsilWNAzFup83XlQ==";
        public const string CUSTOMER_JOB_PROFILE = $"{CUSTOMERS_API_BASE}/customer/job-profile?code=vod9U_H064FUeWaxERAxzC3xzh7SVMtnWhSFfOByA-okAzFumNNJUg==";
        public const string CUSTOMER_FINANCE_ASSETS = $"{CUSTOMERS_API_BASE}/customer/finance-assets/{{customerId}}?code=iFcccGkEcm1mhd2TQjNY4tk6cKT90e68wgOpvTU-46RNAzFuf9r8tw==";
        public const string CUSTOMER_FINANCE_ASSETS_POST = $"{CUSTOMERS_API_BASE}/customer/finance-assets?code=rp3u-kwXRt5U2bXDf-1sgsIcYwPcbzl1XVniXw510b7SAzFui9yHVw==";
        public const string CUSTOMER_WIFE_INFO = $"{CUSTOMERS_API_BASE}/customer/wife-info/{{customerId}}?code=2b9e1uy3xw7MzQooWUvZwS3WsFjHsaffyE2XDlfCdJmYAzFuPszXkw==";

        public static string GetKvkkTextUrl(int id) => KVKK_TEXT.Replace("{id}", id.ToString());
        public static string GetCustomerAddressUrl(string customerId) => CUSTOMER_ADDRESS.Replace("{customerId}", customerId);
        public static string GetCustomerJobInfoUrl(string customerId) => CUSTOMER_JOB_INFO.Replace("{customerId}", customerId);
        public static string GetCustomerFinanceAssetsUrl(string customerId) => CUSTOMER_FINANCE_ASSETS.Replace("{customerId}", customerId);
        public static string GetCustomerWifeInfoUrl(string customerId) => CUSTOMER_WIFE_INFO.Replace("{customerId}", customerId);
    }
}

