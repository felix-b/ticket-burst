import { BeginCheckoutRequest, GrabSeatsReply, GrabSeatsRequest, OrderContract } from "./contracts/backendApi";

export const ServiceClient = {
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
            const response = await fetch('http://localhost:3002/reservation/grab', {
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
        const response = await fetch('http://localhost:3003/checkout/begin', {
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
