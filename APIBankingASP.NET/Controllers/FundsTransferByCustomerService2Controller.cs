﻿using APIBankingASP.NET.Helpers;
using APIBankingASP.NET.Models.FundsTransferByCustomerService2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Web;
using System.Web.Mvc;

namespace APIBankingASP.NET.Controllers
{
    public class FundsTransferByCustomerService2Controller : Controller
    {

        private TransferRequest getTransferRequest(String appID, String customerID, String debitAccountNo, String purposeCode)
        {
            TransferRequest req = new TransferRequest();
            req.uniqueRequestNo = Guid.NewGuid().ToString().Replace("-", "");
            req.appID = appID;
            req.customerID = customerID;
            req.debitAccountNo = debitAccountNo;
            req.transferType = TransferType.NEFT;
            req.transferAmount = 100;
            req.rmtrToBeneInfo = "OnBoarding";
            req.purposeCode = purposeCode;
            return req;
        }

        public ActionResult getBalance()
        {
            GetBalanceRequest req = new GetBalanceRequest();

            req.appID = "299915";
            req.customerID = "299915";
            req.AccountNumber = "000380800000781";
            

            return View(req);
        }

        public ActionResult getStatus()
        {
            GetStatusRequest req = new GetStatusRequest();

            req.appID = "299915";
            req.customerID = "299915";
            req.requestReferenceNo = "012140";

            return View(req);
        }

        // GET: FundsTransferByCustomerService with beneficiary detail
        public ActionResult transferWithBeneDetail()
        {
            TransferRequest req;
            req = getTransferRequest("299915", "299915", "000380800000781", null);
            req.beneficiaryName = "Quantiguous Solutions";
            req.beneficiaryAddress.address1 = "Wilston Road";
            req.beneficiaryAddress.country = "IN";
            req.beneficiaryContact.emailID = "hello@quantiguous.com";
            req.beneficiaryContact.mobileNo = "9561234523";
            req.beneficiaryAccountNo = "026291800001191";
            req.beneficiaryIFSCCode = "HDFC0000001";
            req.beneficiaryMobileForMMID = "9869581569";
            req.beneficiaryMMID = "9532870";

            // transfer with bene detail view
            return View("transferWithBeneDetail", req);

        }

        // GET: FundsTransferByCustomerService with beneficiary code
        public ActionResult transferWithBeneCode()
        {
            TransferRequest req;
            req = getTransferRequest("26528", "26528", "000183200000030", "DOSI");
            req.beneficiaryCode = "ESECURE";

            // transfer with bene code
            return View("transferWithBeneCode", req);
        }


        [HttpPost]
        public ActionResult transfer(TransferRequest request)
        {
            if (TryValidateModel(request) == false)
            {
                return View(request);
            }

            APIBankingASP.NET.Models.Fault fault;

            APIBanking.Environment env = request.buildEnvironment();

            com.quantiguous.ft2.transfer apiReq = new com.quantiguous.ft2.transfer();
            com.quantiguous.ft2.transferResponse apiRep;

            apiReq.uniqueRequestNo = request.uniqueRequestNo;
            apiReq.purposeCode = request.purposeCode;
            apiReq.customerID = request.customerID;
            apiReq.debitAccountNo = request.debitAccountNo;

            //we have an optional element here, pass either beneficiary code or beneficiary detail
            if (request.beneficiaryCode == null)
            {
                com.quantiguous.ft2.beneficiaryType a = new com.quantiguous.ft2.beneficiaryType();
                a.Item = getBeneficiaryDetail(request);
                apiReq.beneficiary = a;
            }
            else
            {
                apiReq.beneficiary = getBeneficiaryCode(request.beneficiaryCode);
            }

            apiReq.transferType = getTransferType(request.transferType);
            apiReq.transferCurrencyCode = com.quantiguous.ft2.currencyCodeType.INR;
            apiReq.transferAmount = (float)request.transferAmount;
            apiReq.remitterToBeneficiaryInfo = request.rmtrToBeneInfo;


            try
            {
                apiRep = FundsTransferByCustomerClient.transfer(env, apiReq);

                TransferResult result = new TransferResult();

                result.version = apiRep.version;
                result.uniqueResponseNo = apiRep.uniqueResponseNo;
                result.attemptNo = int.Parse(apiRep.attemptNo);
                result.transferType = getTransferType(apiRep.transferType);
                result.lowBalanceAlert = apiRep.lowBalanceAlert;

                result.transferStatus.statusCode = getStatusCode(apiRep.transactionStatus.statusCode);
                result.transferStatus.subStatusCode = apiRep.transactionStatus.subStatusCode;
                result.transferStatus.bankReferenceNo = apiRep.transactionStatus.bankReferenceNo;
                result.requestReferenceNo = apiRep.requestReferenceNo;
                return View("transferResult", result);

            }
            /* 
             * the following exceptions have to be caught separately, even when you handle then the same way 
             * this is because of the way the APIBanking.Fault is created
             */
            catch (MessageSecurityException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (FaultException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (Exception e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            return View("fault", fault);

        }

        [HttpPost]
        public ActionResult getbalance(GetBalanceRequest request)
        {
            if (TryValidateModel(request) == false)
            {
                return View(request);
            }

            APIBankingASP.NET.Models.Fault fault;

            APIBanking.Environment env = request.buildEnvironment();

            com.quantiguous.ft2.getBalance apiReq = new com.quantiguous.ft2.getBalance();
            com.quantiguous.ft2.getBalanceResponse apiRep;

            apiReq.appID = request.appID;
            apiReq.customerID = request.customerID;
            apiReq.AccountNumber = request.AccountNumber;
            
            try
            {
                apiRep = FundsTransferByCustomerClient.getBalance(env, apiReq);

                GetBalanceResult result = new GetBalanceResult();

                result.version = apiRep.Version;
                result.accountCurrencyCode = apiRep.accountCurrencyCode.ToString();
                result.accountBalanceAmount = Convert.ToDecimal(apiRep.accountBalanceAmount);
                result.lowBalanceAlert = apiRep.lowBalanceAlert;
                return View("getBalanceResult", result);

            }
            /* 
             * the following exceptions have to be caught separately, even when you handle then the same way 
             * this is because of the way the APIBanking.Fault is created
             */
            catch (MessageSecurityException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (FaultException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (Exception e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            return View("fault", fault);

        }

        [HttpPost]
        public ActionResult getStatus(GetStatusRequest request)
        {
            if (TryValidateModel(request) == false)
            {
                return View(request);
            }

            APIBankingASP.NET.Models.Fault fault;

            APIBanking.Environment env = request.buildEnvironment();

            com.quantiguous.ft2.getStatus apiReq = new com.quantiguous.ft2.getStatus();
            com.quantiguous.ft2.getStatusResponse apiRep;

            apiReq.appID = request.appID;
            apiReq.customerID = request.customerID;
            apiReq.requestReferenceNo = request.requestReferenceNo;

            try
            {
                apiRep = FundsTransferByCustomerClient.getStatus(env, apiReq);

                GetStatusResult result = new GetStatusResult();

                result.version = apiRep.version;
                result.transferType = getTransferType(apiRep.transferType);
                result.reqTransferType = getTransferType(apiRep.reqTransferType);
                result.transactionDate = apiRep.transactionDate;
                result.transferAmount = Convert.ToDecimal(apiRep.transferAmount);
                result.transferCurrencyCode = apiRep.transferCurrencyCode.ToString();
                result.transferStatus.statusCode = getStatusCode(apiRep.transactionStatus.statusCode);
                result.transferStatus.bankReferenceNo = apiRep.transactionStatus.bankReferenceNo;
                result.transferStatus.beneReferenceNo = apiRep.transactionStatus.beneficiaryReferenceNo;
                result.transferStatus.reason = apiRep.transactionStatus.reason;
                return View("getStatusResult", result);

            }
            /* 
             * the following exceptions have to be caught separately, even when you handle then the same way 
             * this is because of the way the APIBanking.Fault is created
             */
            catch (MessageSecurityException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (FaultException e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            catch (Exception e)
            {
                fault = new APIBankingASP.NET.Models.Fault(new APIBanking.Fault(e));
            }
            return View("fault", fault);

        }


        private LastTransferStatus getStatusCode(com.quantiguous.ft2.transactionStatusTypeStatusCode statusCode)
        {
            switch (statusCode)
            {
                // not yet processed, will be done within 1 hour
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.IN_PROCESS:
                    return LastTransferStatus.IN_PROCESS;
                // a failure, reason for failure is in the subStatusCode
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.FAILED:
                    return LastTransferStatus.FAILED;
                // completed, money debited and credite
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.COMPLETED:
                    return LastTransferStatus.COMPLETED;
                // money debited, and sent to beneficiary bank, can still get returned
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.SENT_TO_BENEFICIARY:
                    return LastTransferStatus.SENT_TO_BENEFICIARY;
                // not yet processed, will be done next working day
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.SCHEDULED_FOR_NEXT_WORKDAY:
                    return LastTransferStatus.SCHEDULED_FOR_NEXT_WORKDAY;
                // beneficiary bank returned the money, credited back to your account
                case com.quantiguous.ft2.transactionStatusTypeStatusCode.RETURNED_FROM_BENEFICIARY:
                    return LastTransferStatus.RETURNED_FROM_BENEFICIARY;
            }

            // some default, when in doubt, do not send the money again
            return LastTransferStatus.IN_PROCESS;
        }


        private TransferType? getTransferType(com.quantiguous.ft2.transferTypeType? transferType)
        {
            switch (transferType)
            {
                case com.quantiguous.ft2.transferTypeType.ANY:
                    return TransferType.ANY;
                case com.quantiguous.ft2.transferTypeType.NEFT:
                    return TransferType.NEFT;
                case com.quantiguous.ft2.transferTypeType.IMPS:
                    return TransferType.IMPS;
                case com.quantiguous.ft2.transferTypeType.FT:
                    return TransferType.FT;
            }

            // transferType isn't always going to come, handle that situation
            return null;
        }

        private com.quantiguous.ft2.transferTypeType getTransferType(TransferType transferType)
        {
            switch (transferType)
            {
                case TransferType.ANY:
                    return com.quantiguous.ft2.transferTypeType.ANY;
                case TransferType.NEFT:
                    return com.quantiguous.ft2.transferTypeType.NEFT;
                case TransferType.IMPS:
                    return com.quantiguous.ft2.transferTypeType.IMPS;
                case TransferType.FT:
                    return com.quantiguous.ft2.transferTypeType.FT;
            }

            // some default
            return com.quantiguous.ft2.transferTypeType.ANY;
        }

        private com.quantiguous.ft2.beneficiaryDetailType getBeneficiaryDetail(TransferRequest request)
        {
            com.quantiguous.ft2.beneficiaryDetailType detail = new com.quantiguous.ft2.beneficiaryDetailType();

            com.quantiguous.ft2.AddressType addr = new com.quantiguous.ft2.AddressType();
            com.quantiguous.ft2.contactType contact = new com.quantiguous.ft2.contactType();
            com.quantiguous.ft2.nameType name = new com.quantiguous.ft2.nameType();

            name.Item = request.beneficiaryName;

            detail.beneficiaryName = name;
            addr.address1 = request.beneficiaryAddress.address1;
            addr.address2 = request.beneficiaryAddress.address2;
            addr.address3 = request.beneficiaryAddress.address3;
            addr.postalCode = request.beneficiaryAddress.postalCode;
            addr.city = request.beneficiaryAddress.city;
            addr.stateOrProvince = request.beneficiaryAddress.stateOrProvince;
            addr.country = request.beneficiaryAddress.country;

            contact.mobileNo = request.beneficiaryContact.mobileNo;
            contact.emailID = request.beneficiaryContact.emailID;


            detail.beneficiaryAddress = addr;
            detail.beneficiaryContact = contact;
            detail.beneficiaryAccountNo = request.beneficiaryAccountNo;
            detail.beneficiaryIFSC = request.beneficiaryIFSCCode;
            detail.beneficiaryMMID = request.beneficiaryMMID;
            detail.beneficiaryMobileNo = request.beneficiaryMobileForMMID;
            return detail;
        }

        private com.quantiguous.ft2.beneficiaryType getBeneficiaryCode(String beneficiaryCode)
        {
            com.quantiguous.ft2.beneficiaryType type = new com.quantiguous.ft2.beneficiaryType();
            type.Item = beneficiaryCode;
            return type;
        }
    }
}
