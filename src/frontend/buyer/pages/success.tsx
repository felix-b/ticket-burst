import React from 'react'
import { Alert, AlertDescription, AlertIcon, AlertTitle, Box, Center, Divider, Flex, Grid, GridItem, Heading, Text, VStack } from "@chakra-ui/react"
import { HeaderBar } from '../components/headerBar'
import { createMockPaymentGatewayAPI, CustomerSessionData } from '../components/mockPaymentGatewayLib'

interface SuuccessPageProps {
    data: CustomerSessionData
}

const SuccessPage = (props: SuuccessPageProps) => {

    const { data } = props

    return (
        <>
            <HeaderBar />
            <Center w='full' h='100vh'>
                <Alert maxW='700px'
                    status='success'
                    variant='subtle'
                    flexDirection='column'
                    alignItems='center'
                    justifyContent='center'
                    textAlign='center'
                >
                    <AlertIcon boxSize='40px' mt={5} mr={0} />
                    <AlertDescription mt={4} mb={1} fontSize='3xl' fontWeight={700}>
                        Order Received!
                    </AlertDescription>
                    <AlertDescription maxWidth='md' fontSize='xl' fontWeight={500} mt={4}>
                        {data.customerName}, thank you for the purchase.
                    </AlertDescription>
                    <AlertDescription maxWidth='sm' fontSize='md' mt={2}>
                        Confirmation and the tickets will be sent shortly to the email address you entered in the order:
                    </AlertDescription>
                    <AlertDescription maxWidth='sm' fontSize='xl' mt={2} fontWeight={500}>
                        {data.customerEmail}
                    </AlertDescription>
                    <AlertDescription maxWidth='sm' fontSize='md' mt={2} mb={5}>
                        Order number for tracking purposes: <b>{data.orderNumber}</b>
                    </AlertDescription>
                </Alert>
            </Center>                    
        </>
    )
}

export default SuccessPage

export async function getServerSideProps({ query }): Promise<{ props: SuuccessPageProps }> {
    const sessionId = query.sessionId
    if (typeof sessionId !== 'string' || sessionId.length < 100 || sessionId.length > 1024) {
        throw new Error('Invalid session id')
    }

    const paymentApi = createMockPaymentGatewayAPI();
    const data = await paymentApi.getCustomerSession(sessionId)
    if (!data) {
        throw new Error('Invalid session id')
    }

    return {
        props: {
            data
        }
    }
}
