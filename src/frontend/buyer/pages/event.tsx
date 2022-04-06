import React from 'react'
import { Box, Divider, Flex, Grid, GridItem, Text, VStack } from "@chakra-ui/react"

import { HeaderBar } from '../components/headerBar'
import { EventSearchAreaInfo, EventSearchFullDetail, EventSearchHallInfo, EventSearchResult } from '../contracts/backendApi'
import { Utility } from '../utility'
import Link from 'next/link'
import { EventDetailsSidebar } from '../components/eventDetailsSidebar'
import { ServiceClient } from '../serviceClient'

interface EventPageProps {
    data: EventSearchFullDetail
}

const AreaBox = (props: { 
    eventId: string
    area: EventSearchAreaInfo 
}) => {
    const { eventId, area } = props
    return (
        <Link href={`/area?eventId=${eventId}&areaId=${area.hallAreaId}`}>
            <Box bg='gray.100' p='4px' textAlign='center' cursor='pointer' _hover={{backgroundColor:'orange'}}>
                <Text fontSize='2xl'>{area.name}</Text>
                <Text fontSize='1xl' mb='0' mt='-5px'>{area.availableCapacity}</Text>
            </Box>
        </Link>
    )
}

const SeatingMap = (props: { 
    eventId: string
    hall: EventSearchHallInfo 
}) => {
    const { eventId, hall } = props
    return (
        <Flex mt='40px' wrap='wrap' justify='space-between' gap='5px' maxW='600px'>
            {hall.areas.map(area => <AreaBox key={area.hallAreaId} eventId={eventId} area={area} />)}
        </Flex>
    )
}

const EventPage = (props: EventPageProps) => {

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
                    <Flex wrap='wrap' justify='space-between' gap='40px' align='stretch'>
                        <Box minW='200px' minH='400px'>
                            <EventDetailsSidebar fullDetail={props.data} />
                        </Box>
                        <Box minW='200px' minH='400px'>
                            <Text fontSize='2xl'>Just one step away from choosing your seats!</Text>
                            <Text fontSize='2xl' mt='-5px'>Please pick an area below</Text>
                            <SeatingMap eventId={props.data.event.eventId} hall={props.data.hall} />
                        </Box>
                    </Flex>
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default EventPage

export async function getServerSideProps({ query, res }): Promise<{ props: EventPageProps }> {
    const eventId = query.id
    if (typeof eventId !== 'string' || eventId.length < 10 || eventId.length > 40) {
        throw new Error('Invalid query string')
    }

    try {
        const apiResponse = await fetch(`${ServiceClient.getServiceUrl('search')}/search/event/${eventId}`)
        const envelope = await apiResponse.json()
        const data = envelope.data as EventSearchFullDetail

        res.setHeader(
            'Cache-Control',
            'public, s-maxage=60, stale-while-revalidate=120'
        )

        return { 
            props: { 
                data 
            }
        }
    } catch(err) {
        console.log(err)
        return { 
            props: {
                data: {} as any
            }
        }
    }
}
