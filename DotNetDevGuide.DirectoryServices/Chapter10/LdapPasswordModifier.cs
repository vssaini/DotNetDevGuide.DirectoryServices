using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices.Protocols;
using System.Net;

namespace DotNetDevGuide.DirectoryServices.Chapter10
{
    /// <summary>
    /// Listing 10.16 Modified
    /// </summary>
    public class LdapPasswordModifier : IDisposable
    {
        LdapConnection _connection;

        public LdapPasswordModifier(string server, NetworkCredential creds, bool useSSL)
        {
            _connection = CreateConnection(server, creds, useSSL);
            _connection.Bind();
        }

        private LdapConnection CreateConnection(
            string server,
            NetworkCredential credential,
            bool useSsl
            )
        {
            LdapConnection connect = new LdapConnection(
                new LdapDirectoryIdentifier(server),
                credential
                );

            connect.SessionOptions.ProtocolVersion = 3;
            
            if (useSsl)
            {
                connect.SessionOptions.SecureSocketLayer = true;
                connect.AuthType = AuthType.Basic;
            }
            else
            {
                connect.SessionOptions.Signing = true;
                connect.SessionOptions.Sealing = true;
                connect.AuthType = AuthType.Negotiate;
            }

            return connect;
        }

        public void ChangePassword(
            string userDN,
            string oldPassword,
            string newPassword
            )
        {
            DirectoryAttributeModification deleteMod =
                new DirectoryAttributeModification();

            deleteMod.Name = "unicodePwd";
            deleteMod.Add(GetPasswordData(oldPassword));
            deleteMod.Operation = DirectoryAttributeOperation.Delete;

            DirectoryAttributeModification addMod =
                new DirectoryAttributeModification();

            addMod.Name = "unicodePwd";
            addMod.Add(GetPasswordData(newPassword));
            addMod.Operation = DirectoryAttributeOperation.Add;

            ModifyRequest request = new ModifyRequest(
                userDN,
                deleteMod,
                addMod
                );

            DirectoryResponse response = _connection.SendRequest(request);
        }

        public void SetPassword(
            string userDN,
            string password
            )
        {
            DirectoryAttributeModification pwdMod =
                new DirectoryAttributeModification();
            
            pwdMod.Name = "unicodePwd";
            pwdMod.Add(GetPasswordData(password));
            pwdMod.Operation = DirectoryAttributeOperation.Replace;

            ModifyRequest request = new ModifyRequest(
                userDN,
                pwdMod
                );

            DirectoryResponse response =
                _connection.SendRequest(request);
        }

        private byte[] GetPasswordData(string password)
        {
            string formattedPassword;
            formattedPassword = String.Format("\"{0}\"", password);
            return (Encoding.Unicode.GetBytes(formattedPassword));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }

        #endregion
    }
}
