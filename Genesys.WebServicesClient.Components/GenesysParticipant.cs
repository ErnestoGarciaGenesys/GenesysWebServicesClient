using Genesys.WebServicesClient.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesys.WebServicesClient.Components
{
    public class GenesysParticipant
    {
        readonly ParticipantResource participant;

        public GenesysParticipant(ParticipantResource participant)
        {
            this.participant = participant;
        }

        public string Id { get { return participant.id; } }
        public string Type { get { return participant.type; } }
        public string Nickname { get { return participant.nickname; } }
        public string Visibility { get { return participant.visibility; } }
    }
}
