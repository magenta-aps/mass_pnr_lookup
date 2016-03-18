using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup.Models
{
    public class BatchLine
    {
        [Key]
        public int BatchElementId { get; set; }

        public virtual Batch Batch { get; set; }
        public string SourceContents { get; set; }
        public int Row { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string PNR { get; set; }
        public string Error { get; set; }
        public string MatchedName { get; set; }
        public string MatchedAddress { get; set; }

        public BatchLine()
        { }

        public Queues.LineQueueItem ToQueueItem()
        {
            return new Queues.LineQueueItem() { BatchLineId = this.BatchElementId };
        }

        public SoegInputType1 ToSoegObject()
        {
            var parser = new Parsers.DawaAddressParser();
            var address = parser.ToAddressType(this.Address);
            if (address != null)
            {
                return new CprBroker.Schemas.Part.SoegInputType1()
                {
                    SoegObjekt = new SoegObjektType()
                    {
                        SoegAttributListe = new SoegAttributListeType()
                        {
                            SoegRegisterOplysning = new RegisterOplysningType[]
                            {
                             new RegisterOplysningType()
                             {
                                 Item = new CprBorgerType()
                                 {
                                      FolkeregisterAdresse = address
                                 }
                             }
                            },
                            SoegEgenskab = new SoegEgenskabType[]
                            {
                            new SoegEgenskabType()
                            {
                                NavnStruktur = NavnStrukturType.Create(this.Name)
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                return null;
            }
        }

        public void FillFrom(AttributListeType attr)
        {
            PNR = attr.GetPnr();
            MatchedName = attr.Egenskab.FirstOrDefault()?.NavnStruktur?.PersonNameStructure?.ToString();
            MatchedAddress = ((attr.RegisterOplysning.FirstOrDefault()?.Item as CprBorgerType)?
                .FolkeregisterAdresse?.Item as DanskAdresseType)?.AddressComplete?.AddressPostal?
                .ToAddressString();
            Error = null;
        }

        public void ClearResults()
        {
            PNR = null;
            Error = null;
            MatchedName = null;
            MatchedAddress = null;
        }
    }
}