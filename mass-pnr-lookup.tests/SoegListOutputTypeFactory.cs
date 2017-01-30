using CprBroker.Schemas.Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mass_pnr_lookup.tests
{
    class SoegListOutputTypeFactory
    {
        public SoegListOutputType Create(params string[] names)
        {
            var ret = new SoegListOutputType();

            var objects = names.Select(
                n =>
                {
                    var ooo = new LaesResultatType()
                    {
                        Item = new FiltreretOejebliksbilledeType()
                        {
                            AttributListe = new AttributListeType()
                            {
                                Egenskab = new EgenskabType[] {
                                    new EgenskabType() {
                                        NavnStruktur = new NavnStrukturType() {
                                            PersonNameStructure = new PersonNameStructureType(n.Split(' ')) }
                                        } },
                                RegisterOplysning = new RegisterOplysningType[]
                                {
                                    new RegisterOplysningType() {
                                        Item = new CprBorgerType() {
                                        PersonCivilRegistrationIdentifier = "0123456789"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    return ooo;
                });

            ret.LaesResultat = objects.ToArray();
            return ret;
        }
    }
}
