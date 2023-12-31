﻿using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Reclamation.MultiFactorAuthenticator
{
    public static class MultiFactorAuthenticator
    {
        public static bool AuthenticateSmartCard()
        {
            // Check Smart Card connected
            bool smartCardConnected = true;
            while (smartCardConnected)
            {
                smartCardConnected = IsSmartCardConnected();
            }

            // Get Smart Card info
            CspParameters cspParameters = new CspParameters(1, "Microsoft Base Smart Card Crypto Provider");
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParameters);
            string pubKeyXml = rsaProvider.ToXmlString(false);

            // Find the certficate in the CurrentUser\My store that matches the public key
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            int foundCertsCount = 0;
            X509Certificate2 foundCert = new X509Certificate2();
            foreach (X509Certificate2 cert in x509Store.Certificates)
            {
                if ((cert.PublicKey.Key.ToXmlString(false) == pubKeyXml) && cert.HasPrivateKey)
                {
                    foundCertsCount++;
                    foundCert = cert;
                }
            }

            // Force Smart Card authentication by encrypting and decrypting a string
            if (foundCertsCount == 1)
            {
                string plaintext = "DUMMYTEXT";
                string encryptedstring = Encrypt(foundCert, plaintext);
                string decryptedstring = Decrypt(foundCert, encryptedstring);
                return decryptedstring == plaintext;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Method to check Smart Card is connected
        /// </summary>
        /// <returns></returns>
        public static bool IsSmartCardConnected()
        {
            // Acquire public key stored in the default container of the currently inserted card
            CspParameters cspParameters = new CspParameters(1, "Microsoft Base Smart Card Crypto Provider");
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParameters);
            string pubKeyXml;
            try
            {
                pubKeyXml = rsaProvider.ToXmlString(false);
                return false;
            }
            catch
            {
                string message = "Insert your Smart Card and click OK";
                string title = "Smart Card not found";
                MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                var msgBox = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                if (msgBox == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
                return true;
            }
        }


        /// <summary>
        /// Method to encrypt a string
        /// </summary>
        /// <param name="x509"></param>
        /// <param name="stringToEncrypt"></param>
        /// <returns></returns>
        public static string Encrypt(X509Certificate2 x509, string stringToEncrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringToEncrypt))
            {
                throw new Exception("A x509 certificate and string for encryption must be provided");
            }

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PublicKey.Key;
            byte[] bytestoEncrypt = System.Text.ASCIIEncoding.ASCII.GetBytes(stringToEncrypt);
            byte[] encryptedBytes = rsa.Encrypt(bytestoEncrypt, false);
            return Convert.ToBase64String(encryptedBytes);
        }


        /// <summary>
        /// Method to decrypt a string
        /// </summary>
        /// <param name="x509"></param>
        /// <param name="stringTodecrypt"></param>
        /// <returns></returns>
        public static string Decrypt(X509Certificate2 x509, string stringTodecrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringTodecrypt))
            {
                throw new Exception("A x509 certificate and string for decryption must be provided");
            }

            if (!x509.HasPrivateKey)
            {
                throw new Exception("x509 certicate does not contain a private key for decryption");
            }

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PrivateKey;
            byte[] bytestodecrypt = Convert.FromBase64String(stringTodecrypt);
            byte[] plainbytes = rsa.Decrypt(bytestodecrypt, false);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(plainbytes);
        }
    }
}
