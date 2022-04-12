import { BeginCheckoutRequest, GrabSeatsReply, GrabSeatsRequest, OrderContract } from "./contracts/backendApi";

export type ServiceName = 'search' | 'reservation' | 'checkout'

const urlByServiceName: Record<ServiceName, string> = {
    'search': 'https://3cnuf521pd.execute-api.eu-south-1.amazonaws.com',
    'reservation': 'https://3cnuf521pd.execute-api.eu-south-1.amazonaws.com',
    'checkout': 'https://3cnuf521pd.execute-api.eu-south-1.amazonaws.com',
}

const getServiceUrl = (serviceName: ServiceName): string => {
    return urlByServiceName[serviceName]
}

export const ServiceClient = { 
    
    getServiceUrl,
    
    async grabSeats(request: GrabSeatsRequest): Promise<GrabSeatsReply> {
        const getErrorText = (reply: GrabSeatsReply): string | undefined => {
            if (reply.success === true) {
                return undefined;
            }
            return reply.errorCode === 'SeatNotAvailable'
                ? "Oops, someone took those. We've refreshed the plan. Please try again."
                : "Oops! Something went wrong. Please try again."
        }

        try {
            const response = await fetch(`${getServiceUrl('reservation')}/reservation/grab`, {
                method: 'POST', 
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(request),
            })
            
            const responseJson = await response.json()
            const grabSeatsReply: GrabSeatsReply = responseJson.data as GrabSeatsReply

            return {
                ...grabSeatsReply,
                errorText: getErrorText(grabSeatsReply)
            }

        } catch(err) {
            return { 
                request,
                success: false, 
                errorCode: "exception", 
                errorText: `Oops! Something went wrong: ${err}`,
            }
        }
    },
    
    async beginCheckout(request: BeginCheckoutRequest): Promise<OrderContract | null> {
        const response = await fetch(`${getServiceUrl('checkout')}/checkout/begin`, {
            method: 'POST', 
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(request),
        })
        
        const responseJson = await response.json()
        const order: OrderContract | null = responseJson.data as OrderContract | null

        return order
    }

}
