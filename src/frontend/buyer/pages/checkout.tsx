import { Box, Flex, Grid, GridItem, VStack, Text, Divider, Stat, StatLabel, Tag, TagLabel, StatGroup, HStack, Heading } from "@chakra-ui/react"
import { EventDetailsSidebar } from "../components/eventDetailsSidebar"
import { HeaderBar } from "../components/headerBar"
import { OrderContract, TicketContract } from "../contracts/backendApi"
import { ServiceClient } from "../serviceClient"
import { Utility } from "../utility"


const TicketBasketItem = (props: { ticket: TicketContract }) => {
    const { ticket } = props
    const dateText = Utility.formatIsoDateTimeAsLongDate(ticket.startLocalTime)
    const timeText = Utility.formatIsoDateTimeAsShortTime(ticket.startLocalTime)

    return (
        <StatGroup border={'1px solid darkgray'} p={2} borderRadius={'xl'} w='500px' mb='10px'>
            <HStack w='full' wrap='nowrap' direction='row' justify='space-between' h='120px'>
                <Stat color='blue.700' p='0'>
                    <StatLabel fontSize='xl'>{ticket.eventTitle} {ticket.showTitle}</StatLabel>
                    <StatLabel fontSize='md' mt='-8px'>{dateText} {timeText}</StatLabel>
                    <StatLabel fontSize='lg' mt='8px'>Area {ticket.areaName} Row {ticket.rowName} Seat {ticket.seatName}</StatLabel>
                    <StatLabel fontSize='sm' mt='8px'>{ticket.venueName}</StatLabel>
                    <StatLabel fontSize='sm' mt='-8px'>{ticket.venueAddress}</StatLabel>
                </Stat>
                <Box>
                    <Text fontSize='md'>Price {ticket.priceLevelName}</Text>
                    <Text fontSize='2xl' mt='-10px' fontWeight={600}>${ticket.price}</Text>
                </Box>
                <Divider orientation='vertical' ml='20px' mr='20px' />
                <Box w='30px'>
                    &nbsp;
                </Box>
            </HStack>
        </StatGroup>
    )
}

const CheckoutForm = (props: { order: OrderContract }) => {
    
}

interface CheckoutPageProps {
    eventId: string
    areaId: string
    checkoutToken: string
    success: boolean
    order?: OrderContract
}

const CheckoutPage = (props: CheckoutPageProps) => {
    return (
        <>
            <HeaderBar />
            <Grid 
                minH="100vh"
                templateRows="auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" pt='140px'>
                    <VStack w='full'>
                        <Heading fontSize='4xl' fontWeight={800} color='blue.700'>Checkout</Heading>
                        <Text fontSize='2xl' mt='-5px !important'  color='blue.700'>Just one step away from getting your tickets!</Text>
                        <Flex wrap='nowrap' justify='space-between' gap='40px' align='stretch'>
                            <Box minW='650px' minH='400px'>
                                {props.order && props.order.tickets.map(ticket => <TicketBasketItem key={ticket.id} ticket={ticket} />)}
                            </Box>
                            <Box minW='450px' minH='400px' fontSize='1xl'  color='blue.700'>

                            </Box>
                        </Flex>
                    </VStack>
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default CheckoutPage

export async function getServerSideProps({ query }): Promise<{ props: CheckoutPageProps }> {
    const eventId = query.eventId
    const areaId = query.areaId
    const checkoutToken = query.token

    if (typeof eventId !== 'string' || eventId.length < 10 || eventId.length > 40) {
        throw new Error('Invalid query')
    }
    if (typeof areaId !== 'string' || areaId.length < 10 || areaId.length > 40) {
        throw new Error('Invalid query')
    }
    if (typeof checkoutToken !== 'string' || checkoutToken.length < 100 || checkoutToken.length > 1024) {
        throw new Error('Invalid query')
    }

    try {
        const order = await ServiceClient.beginCheckout({
            preview: true,
            eventId,
            hallAreaId: areaId,
            checkoutToken
        })

        return { 
            props: { 
                eventId,
                areaId,
                checkoutToken,
                success: !!order,
                order
            }
        }
    } catch(err) {
        console.log(err)
        return { 
            props: {
                eventId,
                areaId,
                checkoutToken,
                success: false,
                order: undefined
            }
        }
    }
}
